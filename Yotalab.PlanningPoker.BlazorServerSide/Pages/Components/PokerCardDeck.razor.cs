using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Radzen;
using Yotalab.PlanningPoker.BlazorServerSide.Services;
using Yotalab.PlanningPoker.Grains.Interfaces.Models;

namespace Yotalab.PlanningPoker.BlazorServerSide.Pages.Components
{
  public partial class PokerCardDeck : OwningComponentBase<ParticipantsService>
  {
    private Vote selectedVote;

    [Inject]
    private NotificationService Notification { get; set; }

    [Parameter]
    public Guid SessionId { get; set; }

    [Parameter]
    public Guid ParticipantId { get; set; }

    [Parameter]
    public Vote ParticipantVote { get; set; }

    [Parameter]
    public bool CanVote { get; set; }

    protected override void OnParametersSet()
    {
      this.selectedVote = this.ParticipantVote;
      base.OnParametersSet();
    }

    private async Task HandleSelectedChanged(Vote value)
    {
      if (this.CanVote)
      {
        await this.Service.Vote(this.SessionId, this.ParticipantId, value);
        this.selectedVote = value;
        this.StateHasChanged();
      }
      else
      {
        this.Notification.Notify(new NotificationMessage
        {
          Severity = NotificationSeverity.Warning,
          Summary = "Вы не можете голосовать",
          Detail = "Голосование не началось",
          Duration = 4000
        });
      }
    }
  }
}
