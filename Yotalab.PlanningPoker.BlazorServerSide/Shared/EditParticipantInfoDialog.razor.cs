using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using MudBlazor;
using Yotalab.PlanningPoker.BlazorServerSide.Resources;
using Yotalab.PlanningPoker.BlazorServerSide.Services;
using Yotalab.PlanningPoker.Grains.Interfaces.Models;

namespace Yotalab.PlanningPoker.BlazorServerSide.Shared
{
  public partial class EditParticipantInfoDialog
  {
    private const long MaxAvatarSizeInKb = 300;

    private const long MaxAvatarSizeInBytes = 1024 * MaxAvatarSizeInKb; // 300 кб.

    [Inject]
    IWebHostEnvironment Environment { get; set; }

    [Inject]
    JSInteropFunctions JSInterop { get; set; }

    [Inject]
    ILogger<EditParticipantInfoDialog> Logger { get; set; }

    [CascadingParameter] MudDialogInstance MudDialog { get; set; }

    [Parameter]
    public ParticipantInfo ParticipantInfo { get; set; }

    private ElementReference uploadLabel;

    private string error;

    private static List<string> DefaultAvatarCollection = new List<string>()
    {
        "img/img_avatar_1.png",
        "img/img_avatar_2.png",
        "img/img_avatar_3.png",
        "img/img_avatar_4.png",
        "img/img_avatar_5.png"
    };

    private List<string> fullAvatarCollection = new();

    private string newName;

    private string newAvatarUrl;

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
        this.newAvatarUrl = this.ParticipantInfo.AvatarUrl;
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
      this.error = null;
      this.newAvatarUrl = avatarUrl;
      this.StateHasChanged();
    }

    private async Task Submit()
    {
      if (string.IsNullOrWhiteSpace(this.error))
        await this.Service.ChangeInfo(this.ParticipantInfo.Id, this.newName, this.newAvatarUrl);
      this.MudDialog.Close(DialogResult.Ok(true));
    }

    private void Cancel()
    {
      this.MudDialog.Cancel();
    }

    private string AvatarClassNameByUrl(string avatarUrl)
    {
      return string.Equals(this.newAvatarUrl, avatarUrl, StringComparison.OrdinalIgnoreCase) ?
          "avatar avatar--selected" : "avatar";
    }

    private string AvatarUniqueUrl(string avatarUrl)
    {
      return DefaultAvatarCollection.Contains(avatarUrl) ? avatarUrl : $"{avatarUrl}?{DateTime.UtcNow.Ticks}";
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

        var avatarRelativeDirectoryPath = Path.Combine("img", "avatars");
        var avatarDirectory = Path.Combine(this.Environment.WebRootPath, avatarRelativeDirectoryPath);
        if (!Directory.Exists(avatarDirectory))
          Directory.CreateDirectory(avatarDirectory);
        var avatarUrl = Path.Combine(avatarRelativeDirectoryPath, $"{this.ParticipantInfo.Id}{Path.GetExtension(e.File.Name)}");

        var fullAvatarPath = Path.Combine(this.Environment.WebRootPath, avatarUrl);
        await using FileStream fs = new(fullAvatarPath, FileMode.Create);
        await e.File.OpenReadStream(MaxAvatarSizeInBytes).CopyToAsync(fs);

        this.SelectAvatar(this.AvatarUniqueUrl(avatarUrl));
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

    private bool UploadedAvatarExist(string avatarUrl)
    {
      if (string.IsNullOrWhiteSpace(avatarUrl))
        return false;

      var queryIndex = avatarUrl.IndexOf("?");
      var avatarUrlWithoutQuery = queryIndex > 0 ? avatarUrl.Substring(0, queryIndex) : avatarUrl;

      if (DefaultAvatarCollection.Contains(avatarUrlWithoutQuery) || string.IsNullOrWhiteSpace(avatarUrlWithoutQuery))
        return false;

      var avatarPath = Path.Combine(this.Environment.WebRootPath, avatarUrlWithoutQuery);
      return File.Exists(avatarPath);
    }

    private void InitFullAvatarCollection()
    {
      this.fullAvatarCollection.Clear();
      this.fullAvatarCollection.AddRange(DefaultAvatarCollection);
      if (!DefaultAvatarCollection.Contains(this.newAvatarUrl) && this.UploadedAvatarExist(this.newAvatarUrl))
      {
        this.fullAvatarCollection.Add(this.newAvatarUrl);
      }
    }
  }
}
