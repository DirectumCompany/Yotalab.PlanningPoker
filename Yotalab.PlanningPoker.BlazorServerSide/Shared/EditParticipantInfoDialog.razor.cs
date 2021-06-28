using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Yotalab.PlanningPoker.BlazorServerSide.Services;
using Yotalab.PlanningPoker.Grains.Interfaces.Models;

namespace Yotalab.PlanningPoker.BlazorServerSide.Shared
{
  public partial class EditParticipantInfoDialog
  {
    [CascadingParameter] MudDialogInstance MudDialog { get; set; }

    [Parameter]
    public ParticipantInfo ParticipantInfo { get; set; }

    [Parameter]
    public string Title { get; set; }

    private const string DefaultAvatar = "img/default_avatar.png";

    private static List<string> AvatarCollection = new List<string>()
    {
        "img/img_avatar_1.png",
        "img/img_avatar_2.png",
        "img/img_avatar_3.png",
        "img/img_avatar_4.png",
        "img/img_avatar_5.png",
      };

    private string newName;

    private string newAvatarUrl;

    [Inject]
    private ParticipantsService Service { get; set; }

    public static void Show(IDialogService dialogService, ParticipantInfo participantInfo)
    {
      var parameters = new DialogParameters();
      parameters.Add(nameof(Title), "Изменить имя");
      parameters.Add(nameof(ParticipantInfo), participantInfo);

      var options = new DialogOptions()
      {
        CloseButton = true,
        MaxWidth = MaxWidth.Small,
        FullWidth = true
      };
      dialogService.Show<EditParticipantInfoDialog>("Изменить имя", parameters, options);
    }

    protected override void OnParametersSet()
    {
      if (this.ParticipantInfo != null)
      {
        this.newName = this.ParticipantInfo.Name;
        this.newAvatarUrl = this.ParticipantInfo.AvatarUrl;
      }
      base.OnParametersSet();
    }

    private void SelectAvatar(string avatarUrl)
    {
      this.newAvatarUrl = avatarUrl;
      this.StateHasChanged();
    }

    private async Task Submit()
    {
      await this.Service.ChangeInfo(this.ParticipantInfo.Id, this.newName, this.newAvatarUrl);
      this.MudDialog.Close(DialogResult.Ok(true));
    }

    private void Cancel()
    {
      this.MudDialog.Cancel();
    }
  }
}
