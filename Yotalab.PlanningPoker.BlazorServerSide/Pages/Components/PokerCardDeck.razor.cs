using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Yotalab.PlanningPoker.BlazorServerSide.Services;
using Yotalab.PlanningPoker.Grains.Interfaces.Models;

namespace Yotalab.PlanningPoker.BlazorServerSide.Pages.Components
{
  public partial class PokerCardDeck : OwningComponentBase<ParticipantsService>
  {
    private Vote selectedVote;

    [Inject]
    private ISnackbar Snackbar { get; set; }

    [Parameter]
    public Guid SessionId { get; set; }

    [Parameter]
    public Guid ParticipantId { get; set; }

    [Parameter]
    public Vote ParticipantVote { get; set; }

    [Parameter]
    public Bulletin Bulletin { get; set; }

    [Parameter]
    public bool CanVote { get; set; }

    [Parameter]
    public string CanNotVoteMessage { get; set; }

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
      else if (!string.IsNullOrEmpty(this.CanNotVoteMessage))
      {
        this.Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopCenter;
        this.Snackbar.Add(this.CanNotVoteMessage, Severity.Warning, options =>
        {
          options.SnackbarVariant = Variant.Filled;
          options.VisibleStateDuration = 2000;
          options.ShowTransitionDuration = 200;
          options.HideTransitionDuration = 200;
        });
      }
    }
  }
}
