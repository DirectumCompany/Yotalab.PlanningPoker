using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using Yotalab.PlanningPoker.Grains.Interfaces;
using Yotalab.PlanningPoker.Grains.Interfaces.Models;
using Yotalab.PlanningPoker.Grains.Interfaces.Models.Notifications;

namespace Yotalab.PlanningPoker.Grains
{
  /// <summary>
  /// Грейн представляет собой сессию планирования.
  /// </summary>
  public class SessionGrain : Grain, ISessionGrain
  {
    private readonly IPersistentState<SessionGrainState> grainState;

    public SessionGrain([PersistentState("Session")] IPersistentState<SessionGrainState> grainState)
    {
      this.grainState = grainState;
    }

    #region ISessionGrain

    public Task AcceptVote(Guid participantId, Vote vote)
    {
      var processingState = this.grainState.State.ProcessingState;
      if (processingState != SessionProcessingState.Stopped && processingState != SessionProcessingState.Started)
        throw new InvalidOperationException($"Cannot accept vore for not started session. Session processing state: {processingState}");

      this.grainState.State.ParticipantVotes[participantId] = vote;

      this.NotifyAcceptVote(participantId, vote);

      return this.grainState.WriteStateAsync();
    }

    public Task CreateAsync(string name, IParticipantGrain moderator)
    {
      var moderatorId = moderator.GetPrimaryKey();
      this.grainState.State.Name = name;
      this.grainState.State.ModeratorId = moderatorId;
      this.grainState.State.ProcessingState = SessionProcessingState.Initial;
      return this.grainState.WriteStateAsync();
    }

    public Task Enter(Guid participantId)
    {
      this.grainState.State.ParticipantVotes[participantId] = Vote.Unset;

      this.NotifyNewParticipantEntered(participantId);

      return this.grainState.WriteStateAsync();
    }

    public Task Exit(Guid participantId)
    {
      if (this.grainState.State.ParticipantVotes.ContainsKey(participantId))
      {
        this.grainState.State.ParticipantVotes.Remove(participantId);

        this.NotifyParticipantExit(participantId);

        return this.grainState.WriteStateAsync();
      }

      return Task.CompletedTask;
    }

    public Task FinishAsync(Guid initiatorId)
    {
      var processingState = this.grainState.State.ProcessingState;
      if (processingState != SessionProcessingState.Stopped && processingState != SessionProcessingState.Started)
        throw new InvalidOperationException($"Cannot finish not started session. Session processing state: {processingState}");

      this.grainState.State.ProcessingState = SessionProcessingState.Finished;
      this.ReplaceUnsetVoteToIDontKnownVote();

      this.NotifyProcessingStateChanged(this.grainState.State.ProcessingState);

      return this.grainState.WriteStateAsync();
    }

    public Task ResetAsync(Guid initiatorId)
    {
      var processingState = this.grainState.State.ProcessingState;
      if (processingState != SessionProcessingState.Finished)
        throw new InvalidOperationException($"Cannot reset not finished session. Session processing state: {processingState}");

      this.grainState.State.ProcessingState = SessionProcessingState.Initial;
      this.ResetParticipantVotes();

      this.NotifyProcessingStateChanged(this.grainState.State.ProcessingState);

      return this.grainState.WriteStateAsync();
    }

    public Task StartAsync(Guid initiatorId)
    {
      var processingState = this.grainState.State.ProcessingState;
      if (processingState != SessionProcessingState.Initial)
        throw new InvalidOperationException($"Cannot start not initial session. Session processing state: {processingState}");

      this.grainState.State.ProcessingState = SessionProcessingState.Started;

      this.NotifyProcessingStateChanged(this.grainState.State.ProcessingState);

      return this.grainState.WriteStateAsync();
    }

    public Task<SessionInfo> StatusAsync()
    {
      return Task.FromResult(this.ToInfo());
    }

    public Task StopAsync(Guid initiatorId)
    {
      var processingState = this.grainState.State.ProcessingState;
      if (processingState != SessionProcessingState.Started)
        throw new InvalidOperationException($"Cannot stop not started session. Session processing state: {processingState}");

      this.grainState.State.ProcessingState = SessionProcessingState.Stopped;

      this.NotifyProcessingStateChanged(this.grainState.State.ProcessingState);

      return this.grainState.WriteStateAsync();
    }

    public Task<ImmutableArray<ParticipantVote>> ParticipantVotes()
    {
      return Task.FromResult(this.grainState.State
        .ParticipantVotes
        .Select(v => new ParticipantVote()
        {
          ParticipantId = v.Key,
          Vote = v.Value
        })
        .ToImmutableArray());
    }

    #endregion

    #region Базовый класс

    public override Task OnActivateAsync()
    {
      if (this.grainState.State.ParticipantVotes == null)
        this.grainState.State.ParticipantVotes = new Dictionary<Guid, Vote>();

      return base.OnActivateAsync();
    }

    #endregion

    #region Методы

    private void NotifyProcessingStateChanged(SessionProcessingState newProcessingState)
    {
      var sessionId = this.GetPrimaryKey();
      this.GetStreamProvider("SMS").GetStream<SessionProcessingNotification>(sessionId, typeof(SessionProcessingNotification).FullName)
        .OnNextAsync(new SessionProcessingNotification(sessionId, newProcessingState))
        .Ignore();
    }

    private void NotifyNewParticipantEntered(Guid participantId)
    {
      var sessionId = this.GetPrimaryKey();
      this.GetStreamProvider("SMS").GetStream<ParticipantsChangedNotification>(sessionId, typeof(ParticipantsChangedNotification).FullName)
        .OnNextAsync(new ParticipantsChangedNotification(sessionId, new HashSet<Guid>() { participantId }, new HashSet<Guid>()))
        .Ignore();
    }

    private void NotifyParticipantExit(Guid participantId)
    {
      var sessionId = this.GetPrimaryKey();
      this.GetStreamProvider("SMS").GetStream<ParticipantsChangedNotification>(sessionId, typeof(ParticipantsChangedNotification).FullName)
        .OnNextAsync(new ParticipantsChangedNotification(sessionId, new HashSet<Guid>(), new HashSet<Guid>() { participantId }))
        .Ignore();
    }

    private void NotifyAcceptVote(Guid participantId, Vote vote)
    {
      var sessionId = this.GetPrimaryKey();
      this.GetStreamProvider("SMS").GetStream<VoteNotification>(sessionId, typeof(VoteNotification).FullName)
        .OnNextAsync(new VoteNotification(sessionId, participantId, vote))
        .Ignore();
    }

    private void ResetParticipantVotes()
    {
      var keys = this.grainState.State.ParticipantVotes.Keys.ToList();
      foreach (var voteKey in keys)
        this.grainState.State.ParticipantVotes[voteKey] = Vote.Unset;
    }

    private void ReplaceUnsetVoteToIDontKnownVote()
    {
      var participantsIds = this.grainState.State.ParticipantVotes.Keys.ToList();
      foreach (var participantId in participantsIds)
      {
        var vote = this.grainState.State.ParticipantVotes[participantId];
        if (Vote.Unset.Equals(vote))
          this.grainState.State.ParticipantVotes[participantId] = Vote.IDontKnown;
      }
    }

    private SessionInfo ToInfo()
    {
      return new SessionInfo()
      {
        Id = this.GetPrimaryKey(),
        ModeratorId = this.grainState.State.ModeratorId,
        Name = this.grainState.State.Name,
        ProcessingState = this.grainState.State.ProcessingState,
        ParticipantsCount = this.grainState.State.ParticipantVotes.Keys.Count
      };
    }

    #endregion
  }

  public class SessionGrainState
  {
    public string Name { get; set; }

    public Guid ModeratorId { get; set; }

    public SessionProcessingState ProcessingState { get; set; }

    public Dictionary<Guid, Vote> ParticipantVotes { get; set; }
  }
}
