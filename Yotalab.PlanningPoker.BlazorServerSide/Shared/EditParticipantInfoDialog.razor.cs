using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using MudBlazor;
using Yotalab.PlanningPoker.BlazorServerSide.Resources;
using Yotalab.PlanningPoker.BlazorServerSide.Services;
using Yotalab.PlanningPoker.BlazorServerSide.Services.FilesStoraging;
using Yotalab.PlanningPoker.Grains.Interfaces.Models;

namespace Yotalab.PlanningPoker.BlazorServerSide.Shared
{
  public partial class EditParticipantInfoDialog
  {
    private const long MaxAvatarSizeInKb = 300;

    private const long MaxAvatarSizeInBytes = 1024 * MaxAvatarSizeInKb; // 300 кб.

    [Inject]
    JSInteropFunctions JSInterop { get; set; }

    [Inject]
    ILogger<EditParticipantInfoDialog> Logger { get; set; }

    [Inject]
    AvatarStorage AvatarStorage { get; set; }

    [CascadingParameter] MudDialogInstance MudDialog { get; set; }

    [Parameter]
    public ParticipantInfo ParticipantInfo { get; set; }

    private ElementReference uploadLabel;

    private string error;

    private List<string> fullAvatarCollection = new();

    private string newName;

    private AvatarDescriptor newAvatarDescriptor;

    private TransientAvatarDescriptor transientAvatarDescriptor;

    private bool shouldRender = true;

    [Inject]
    private ParticipantsService Service { get; set; }

    public static void Show(IDialogService dialogService, ParticipantInfo participantInfo)
    {
      var parameters = new DialogParameters();
      parameters.Add(nameof(ParticipantInfo), participantInfo);

      var options = new DialogOptions()
      {
        CloseButton = true,
        MaxWidth = MaxWidth.Small,
        FullWidth = true
      };
      dialogService.Show<EditParticipantInfoDialog>(UIResources.EditParticipantInfoDialogTitle, parameters, options);
    }

    protected override void OnParametersSet()
    {
      if (this.ParticipantInfo != null)
      {
        this.newName = this.ParticipantInfo.Name;
        this.newAvatarDescriptor = !string.IsNullOrWhiteSpace(this.ParticipantInfo.AvatarUrl)
          ? new AvatarDescriptor(this.ParticipantInfo.AvatarUrl)
          : null;
        this.InitFullAvatarCollection();
      }
      base.OnParametersSet();
    }

    protected override bool ShouldRender()
    {
      return this.shouldRender;
    }

    private void SelectAvatar(string avatarUrl)
    {
      if (string.IsNullOrEmpty(avatarUrl))
        return;

      this.error = null;
      this.newAvatarDescriptor = new AvatarDescriptor(avatarUrl);
      this.StateHasChanged();
    }

    private async Task Submit()
    {
      if (string.IsNullOrWhiteSpace(this.error))
      {
        try
        {
          var oldAvatarUrl = this.ParticipantInfo.AvatarUrl;
          var newAvatarUrl = await this.HandleAvatarChanges();

          if (!string.Equals(this.ParticipantInfo.Name, this.newName) || !string.Equals(oldAvatarUrl, newAvatarUrl))
            await this.Service.ChangeInfo(this.ParticipantInfo.Id, this.newName, newAvatarUrl);
        }
        catch (Exception ex)
        {
          this.Logger.LogError(ex, "Can not submit profile changes (participant id: {ParticipantId})", this.ParticipantInfo?.Id);
          this.error = UIResources.EditParticipantInfoDialogAvatarSubmitError;
        }
      }
      this.MudDialog.Close(DialogResult.Ok(true));
    }

    private void Cancel()
    {
      try
      {
        this.CancelAvatarChanges();
      }
      catch (Exception ex)
      {
        this.Logger.LogError(ex, "Can not cancel profile changes (participant id: {ParticipantId})", this.ParticipantInfo?.Id);
      }
      this.MudDialog.Cancel();
    }

    private string AvatarClassNameByUrl(string avatarUrl)
    {
      return string.Equals(this.newAvatarDescriptor?.RelativeFileName, avatarUrl, StringComparison.OrdinalIgnoreCase) ?
          "avatar avatar--selected" : "avatar";
    }

    private async Task StartUploading()
    {
      await this.JSInterop.ClickElement(this.uploadLabel);
    }

    private async Task Upload(InputFileChangeEventArgs e)
    {
      this.error = null;
      if (e.File.Size > MaxAvatarSizeInBytes)
      {
        this.error = string.Format(UIResources.EditParticipantInfoDialogMaxAvatarSize, MaxAvatarSizeInKb);
        return;
      }

      try
      {
        this.shouldRender = false;

        if (this.transientAvatarDescriptor != null)
          this.AvatarStorage.DeleteTransient(this.transientAvatarDescriptor);

        this.transientAvatarDescriptor = await this.AvatarStorage.WriteTransient(
          e.File.OpenReadStream(MaxAvatarSizeInBytes),
          this.ParticipantInfo.Id,
          e.File.Name);

        this.SelectAvatar(this.transientAvatarDescriptor.RelativeFileName);
        this.InitFullAvatarCollection();
      }
      catch (Exception ex)
      {
        this.Logger.LogError(ex, "{FileName} not uploaded: {Message}", e.File.Name, ex.Message);
        this.error = UIResources.EditParticipantInfoDialogAvatarUploadError;
      }
      finally
      {
        this.shouldRender = true;
      }
    }

    private void InitFullAvatarCollection()
    {
      this.fullAvatarCollection.Clear();
      this.fullAvatarCollection.AddRange(AvatarStorage.DefaultAvatarCollection);

      if (this.newAvatarDescriptor != null && !this.fullAvatarCollection.Contains(this.newAvatarDescriptor.RelativeFileName))
        this.fullAvatarCollection.Add(this.newAvatarDescriptor.RelativeFileName);
    }

    private async Task<string> HandleAvatarChanges()
    {
      var newAvatarUrl = this.newAvatarDescriptor?.RelativeFileName;

      // Попробуем получить старый аватар, если он был не из коллекции по умолчанию.
      var oldAvatarUrl = this.ParticipantInfo.AvatarUrl;
      var oldNotDefaultAvatarDescriptor = !string.IsNullOrWhiteSpace(oldAvatarUrl)
        && !AvatarStorage.DefaultAvatarCollection.Contains(oldAvatarUrl) ? new AvatarDescriptor(oldAvatarUrl) : null;

      // Удалим аватар если он был не из коллекции по умолчанию
      //  и выбрали новый из коллекции по умолчанию.
      bool needCleanOldAvatar = AvatarStorage.DefaultAvatarCollection.Contains(newAvatarUrl) && oldNotDefaultAvatarDescriptor != null;

      if (this.transientAvatarDescriptor != null)
      {
        if (this.transientAvatarDescriptor.RelativeFileName.Equals(this.newAvatarDescriptor?.RelativeFileName))
        {
          await this.AvatarStorage.CommitTransient(this.transientAvatarDescriptor);
          newAvatarUrl = AvatarDescriptor.UniqueUrl(this.transientAvatarDescriptor.TargetRelativeFileName);

          // Удалим старый аватар если он был не из коллекции по умолчанию
          //  и новый загруженный аватар отличается именем.
          needCleanOldAvatar = oldNotDefaultAvatarDescriptor != null
            && !this.transientAvatarDescriptor.TargetRelativeFileName.Equals(oldNotDefaultAvatarDescriptor.RelativeFileName);
        }
        else
          this.AvatarStorage.DeleteTransient(this.transientAvatarDescriptor);
      }
      this.transientAvatarDescriptor = null;

      if (needCleanOldAvatar)
        this.AvatarStorage.DeleteAvatar(oldNotDefaultAvatarDescriptor);

      return newAvatarUrl;
    }

    private void CancelAvatarChanges()
    {
      if (this.transientAvatarDescriptor != null)
      {
        this.AvatarStorage.DeleteTransient(this.transientAvatarDescriptor);
        this.transientAvatarDescriptor = null;
      }
    }
  }
}
