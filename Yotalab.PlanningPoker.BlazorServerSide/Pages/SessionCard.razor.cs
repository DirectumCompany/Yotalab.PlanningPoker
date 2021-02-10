using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Streams;
using Yotalab.PlanningPoker.BlazorServerSide.Pages.Components;
using Yotalab.PlanningPoker.BlazorServerSide.Services;
using Yotalab.PlanningPoker.BlazorServerSide.Services.DTO;
using Yotalab.PlanningPoker.Grains.Interfaces.Models;
using Yotalab.PlanningPoker.Grains.Interfaces.Models.Notifications;

namespace Yotalab.PlanningPoker.BlazorServerSide.Pages
{
  public partial class SessionCard : AuthorizedOwningComponentBase<SessionService>
  {
    private Vote participantVote;
    private SessionInfo session;
    private IReadOnlyCollection<ParticipantInfoDTO> participantVotes;
    private bool userNotJoinedToSession = false;
    private StreamSubscriptionHandle<SessionProcessingNotification> sessionProcessingSubscription;
    private StreamSubscriptionHandle<ParticipantsChangedNotification> participantsChangedSubscription;
    private StreamSubscriptionHandle<VoteNotification> voteSubscription;
    private StreamSubscriptionHandle<ParticipantChangedNotification> participantChangedSubscription;

    [Parameter]
    public Guid SessionId { get; set; }

    protected override async Task OnInitializedAsync()
    {
      await base.OnInitializedAsync();
      await this.RefreshAsync();

      this.sessionProcessingSubscription = await this.Service.SubscribeAsync<SessionProcessingNotification>(this.SessionId, notification => this.InvokeAsync(this.HandleNotification));
      this.participantsChangedSubscription = await this.Service.SubscribeAsync<ParticipantsChangedNotification>(this.SessionId, notification => this.InvokeAsync(this.HandleNotification));
      this.voteSubscription = await this.Service.SubscribeAsync<VoteNotification>(this.SessionId, notification => this.InvokeAsync(() => this.HandleVoteNotification(notification)));
      this.participantChangedSubscription = await this.ScopedServices.GetRequiredService<ParticipantsService>()
        .SubscribeAsync(notification => this.InvokeAsync(this.HandleNotification));
    }

    private async Task RefreshAsync()
    {
      this.session = await this.Service.GetAsync(this.SessionId);
      this.participantVotes = await this.Service.ListParticipants(this.SessionId);
      this.userNotJoinedToSession = !await this.Service.ParticipantJoined(this.SessionId, this.ParticipantId);
      this.participantVote = this.participantVotes.SingleOrDefault(p => p.Id == this.ParticipantId)?.Vote;
    }

    private async Task HandleNotification()
    {
      await this.RefreshAsync();
      this.StateHasChanged();
    }

    private Task HandleVoteNotification(VoteNotification notification)
    {
      foreach (var participant in this.participantVotes)
      {
        if (participant.Id == notification.ParticipantId)
        {
          participant.Vote = notification.Vote;
        }

        if (this.ParticipantId == notification.ParticipantId)
        {
          this.participantVote = notification.Vote;
        }
      }

      this.StateHasChanged();
      return Task.CompletedTask;
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      if (this.IsDisposed)
        return;

      if (disposing)
      {
        try
        {
          this.sessionProcessingSubscription?.UnsubscribeAsync();
          this.participantsChangedSubscription?.UnsubscribeAsync();
          this.voteSubscription?.UnsubscribeAsync();
          this.participantChangedSubscription?.UnsubscribeAsync();
        }
        catch
        {
          // Игнорируем.
        }
      }
    }
  }
}
