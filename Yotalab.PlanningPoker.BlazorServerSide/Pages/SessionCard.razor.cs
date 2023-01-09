using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using Orleans.Streams;
using Yotalab.PlanningPoker.BlazorServerSide.Pages.Components;
using Yotalab.PlanningPoker.BlazorServerSide.Resources;
using Yotalab.PlanningPoker.BlazorServerSide.Services;
using Yotalab.PlanningPoker.BlazorServerSide.Services.Args;
using Yotalab.PlanningPoker.BlazorServerSide.Services.DTO;
using Yotalab.PlanningPoker.Grains.Interfaces.Models;
using Yotalab.PlanningPoker.Grains.Interfaces.Models.Notifications;

namespace Yotalab.PlanningPoker.BlazorServerSide.Pages
{
  public partial class SessionCard : AuthorizedOwningComponentBase<SessionService>, IAsyncDisposable
  {
    private EditSessionArgs editSessionArgs;
    private Vote participantVote;
    private SessionInfo session;
    private Bulletin bulletin;
    private IReadOnlyCollection<ParticipantInfoDTO> participantVotes = new List<ParticipantInfoDTO>();
    private bool userNotJoinedToSession = false;
    private StreamSubscriptionHandle<SessionProcessingNotification> sessionProcessingSubscription;
    private StreamSubscriptionHandle<ParticipantsChangedNotification> participantsChangedSubscription;
    private StreamSubscriptionHandle<VoteNotification> voteSubscription;
    private StreamSubscriptionHandle<ParticipantChangedNotification> participantChangedSubscription;
    private StreamSubscriptionHandle<SessionInfoChangedNotification> sessionInfoChangedSubscription;
    private StreamSubscriptionHandle<SessionRemovedNotification> sessionRemovedSubscription;

    [Parameter]
    public Guid SessionId { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; }

    [Inject]
    private IDialogService DialogService { get; set; }

    protected override async Task OnInitializedAsync()
    {
      await base.OnInitializedAsync();
      await this.RefreshAsync();

      // Обработка изменения состояния сессии (начало голосования, остановка, и т.п.).
      this.sessionProcessingSubscription = await this.Service.SubscribeAsync<SessionProcessingNotification>(this.SessionId,
        notification => this.InvokeAsync(this.HandleNotification));

      // Обработка изменения участников (кого-то добавили, кого-то удалили и т.п.).
      this.participantsChangedSubscription = await this.Service.SubscribeAsync<ParticipantsChangedNotification>(this.SessionId,
        notification => this.InvokeAsync(this.HandleNotification));

      // Обработка появления нового голоса.
      this.voteSubscription = await this.Service.SubscribeAsync<VoteNotification>(this.SessionId,
        notification => this.InvokeAsync(() => this.HandleVoteNotification(notification)));

      // Обработка изменения одного из участников.
      this.participantChangedSubscription = await this.ScopedServices.GetRequiredService<ParticipantsService>()
        .SubscribeAsync(this.TryRefreshParticipantChanges);

      // Обработка изменения информации о сессии.
      this.sessionInfoChangedSubscription = await this.Service.SubscribeAsync<SessionInfoChangedNotification>(this.SessionId,
        _ => this.InvokeAsync(this.HandleNotification));

      // Обработка удаления сессии.
      this.sessionRemovedSubscription = await this.Service.SubscribeAsync<SessionRemovedNotification>(this.SessionId,
        _ => this.InvokeAsync(() => this.NavigationManager.NavigateTo("/")));
    }

    private Task TryRefreshParticipantChanges(ParticipantChangedNotification arg)
    {
      // Если в списке участников есть тот, чья информация поменялась, то обновим.
      return this.participantVotes.Any(p => p.Id == arg.ChangedInfo.Id) ?
        this.InvokeAsync(this.HandleNotification) :
        Task.CompletedTask;
    }

    private async Task RefreshAsync()
    {
      this.session = await this.Service.GetAsync(this.SessionId);
      if (session.IsInitialized)
      {
        this.participantVotes = await this.Service.ListParticipants(this.SessionId);
        this.userNotJoinedToSession = !await this.Service.ParticipantJoined(this.SessionId, this.ParticipantId);
        this.participantVote = this.participantVotes.SingleOrDefault(p => p.Id == this.ParticipantId)?.Vote;
        this.bulletin = await this.Service.GetBulletin(this.SessionId);
      }
      else
      {
        this.participantVotes = new List<ParticipantInfoDTO>();
        this.userNotJoinedToSession = true;
        this.participantVote = Vote.Unset;
      }
    }

    private async Task HandleNotification()
    {
      await this.RefreshAsync();
      if (this.userNotJoinedToSession)
        this.NavigationManager.NavigateTo("/");
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

    private Task HandleEditSessionConfirmAsync(EditSessionArgs args)
    {
      return this.Service.EditSessionAsync(args);
    }

    private void OnShowEditModal()
    {
      this.editSessionArgs = new EditSessionArgs()
      {
        Name = this.session?.Name,
        SessionId = this.SessionId,
        AutoStop = this.session?.AutoStop == true,
        Bulletin = new Bulletin(this.bulletin)
      };

      var onConfirm = new EventCallbackFactory().Create<EditSessionArgs>(this, this.HandleEditSessionConfirmAsync);
      EditSessionDialog.Show(this.DialogService, UIResources.EditSessionDialogTitle, this.editSessionArgs, onConfirm);
    }

    public bool CanVote()
    {
      return
        !this.session.ObserverIds.Contains(this.ParticipantId) &&
        (this.session.ProcessingState == SessionProcessingState.Started ||
        this.session.ProcessingState == SessionProcessingState.Stopped);
    }

    public string CanNotVoteMessage()
    {
      if (this.session.ObserverIds.Contains(this.ParticipantId))
        return UIResources.YouObserverAndCanNotVote;

      return UIResources.NotStartedSessionState;
    }

    public async ValueTask DisposeAsync()
    {
      try
      {
        await this.sessionProcessingSubscription?.UnsubscribeAsync();
        await this.participantsChangedSubscription?.UnsubscribeAsync();
        await this.voteSubscription?.UnsubscribeAsync();
        await this.participantChangedSubscription?.UnsubscribeAsync();
        await this.sessionInfoChangedSubscription?.UnsubscribeAsync();
        await this.sessionRemovedSubscription?.UnsubscribeAsync();
      }
      catch
      {
        // Игнорируем.
      }
    }
  }
}
