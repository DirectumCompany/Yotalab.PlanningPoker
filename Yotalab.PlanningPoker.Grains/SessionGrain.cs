using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using Yotalab.PlanningPoker.Grains.Interfaces;
using Yotalab.PlanningPoker.Grains.Interfaces.Models;
using Yotalab.PlanningPoker.Grains.Interfaces.Models.Args;
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

      this.TryAutostopSession();

      return this.grainState.WriteStateAsync();
    }

    public Task CreateAsync(string name, IParticipantGrain moderator, bool autostop, Bulletin bulletin)
    {
      var moderatorId = moderator.GetPrimaryKey();
      this.grainState.State.Name = name;
      this.grainState.State.ModeratorIds.Add(moderatorId);
      this.grainState.State.ProcessingState = SessionProcessingState.Initial;
      this.grainState.State.AutoStop = autostop;
      this.grainState.State.Bulletin = bulletin;
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
      if (!this.grainState.State.ParticipantVotes.ContainsKey(participantId))
        return Task.CompletedTask;

      this.grainState.State.ParticipantVotes.Remove(participantId);
      this.grainState.State.ObserverIds.Remove(participantId);

      // После удаления участника, удалим его из модераторов, если он там был.
      if (this.grainState.State.ModeratorIds.Contains(participantId))
      {
        this.grainState.State.ModeratorIds.Remove(participantId);
        if (this.grainState.State.ModeratorIds.Count == 0)
        {
          // Назначим первого попавшего пользователя модератором.
          var firstParticipantId = this.grainState.State.ParticipantVotes.Keys.FirstOrDefault();
          if (firstParticipantId != default)
            this.grainState.State.ModeratorIds.Add(firstParticipantId);
        }
      }

      this.NotifyParticipantExit(participantId);

      return this.grainState.WriteStateAsync();
    }

    public async Task Kick(Guid participantId, Guid initiatorId)
    {
      if (!this.grainState.State.ModeratorIds.Contains(initiatorId))
        throw new InvalidOperationException($"Only moderator can kick participant. Initiator: {initiatorId}");

      if (!this.grainState.State.ParticipantVotes.ContainsKey(participantId))
        return;

      var participantGrain = this.GrainFactory.GetGrain<IParticipantGrain>(participantId);
      using var _ = RequestContext.AllowCallChainReentrancy();
      await participantGrain.Leave(this.GetPrimaryKey());
    }

    public Task FinishAsync(Guid initiatorId)
    {
      if (!this.grainState.State.ModeratorIds.Contains(initiatorId))
        throw new InvalidOperationException($"Only moderator can change session processing state. Initiator: {initiatorId}");

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
      return this.ResetAsync(initiatorId, false);
    }

    public Task ResetAsync(Guid initiatorId, bool startImmediately)
    {
      if (!this.grainState.State.ModeratorIds.Contains(initiatorId))
        throw new InvalidOperationException($"Only moderator can change session processing state. Initiator: {initiatorId}");

      var processingState = this.grainState.State.ProcessingState;
      if (processingState != SessionProcessingState.Finished)
        throw new InvalidOperationException($"Cannot reset not finished session. Session processing state: {processingState}");

      this.grainState.State.ProcessingState = startImmediately ? SessionProcessingState.Started : SessionProcessingState.Initial;
      this.ResetParticipantVotes();

      this.NotifyProcessingStateChanged(this.grainState.State.ProcessingState);

      return this.grainState.WriteStateAsync();
    }

    public Task StartAsync(Guid initiatorId)
    {
      if (!this.grainState.State.ModeratorIds.Contains(initiatorId))
        throw new InvalidOperationException($"Only moderator can change session processing state. Initiator: {initiatorId}");

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
      if (!this.grainState.State.ModeratorIds.Contains(initiatorId))
        throw new InvalidOperationException($"Only moderator can change session processing state. Initiator: {initiatorId}");

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

    public Task AddModerator(Guid participantId)
    {
      if (!this.grainState.State.ParticipantVotes.ContainsKey(participantId))
        throw new InvalidOperationException($"Can not add moderator. Participant {participantId} not joined to session {this.GetPrimaryKey()}");

      this.grainState.State.ModeratorIds.Add(participantId);

      this.NotifyModeratorsChanged(new HashSet<Guid>() { participantId }, new HashSet<Guid>());

      return this.grainState.WriteStateAsync();
    }

    public Task RemoveModerator(Guid participantId)
    {
      if (this.grainState.State.ModeratorIds.Count == 1)
        throw new InvalidOperationException($"Can not remove single moderator. Session {this.GetPrimaryKey()}, Moderator {this.grainState.State.ModeratorIds.First()}");

      this.grainState.State.ModeratorIds.Remove(participantId);

      this.NotifyModeratorsChanged(new HashSet<Guid>(), new HashSet<Guid>() { participantId });

      return this.grainState.WriteStateAsync();
    }

    public Task AddObserver(Guid participantId, Guid initiatorId)
    {
      if (!this.grainState.State.ModeratorIds.Contains(initiatorId))
        throw new InvalidOperationException($"Only moderator can add observer. Initiator: {initiatorId}");

      this.grainState.State.ObserverIds.Add(participantId);

      this.NotifyObserversChanged(new HashSet<Guid>() { participantId }, new HashSet<Guid>());

      return this.grainState.WriteStateAsync();
    }

    public Task RemoveObserver(Guid participantId, Guid initiatorId)
    {
      if (!this.grainState.State.ModeratorIds.Contains(initiatorId))
        throw new InvalidOperationException($"Only moderator can remove observer. Initiator: {initiatorId}");

      this.grainState.State.ObserverIds.Remove(participantId);

      this.NotifyObserversChanged(new HashSet<Guid>(), new HashSet<Guid>() { participantId });

      return this.grainState.WriteStateAsync();
    }

    public Task ChangeInfo(ChangeSessionInfoArgs args)
    {
      this.grainState.State.Name = args.Name;
      this.grainState.State.AutoStop = args.AutoStop;

      this.NotifySessionInfoChanged();

      return this.grainState.WriteStateAsync();
    }

    public async Task Remove(Guid initiatorId)
    {
      if (!this.grainState.State.ModeratorIds.Contains(initiatorId))
        throw new InvalidOperationException($"Only moderator can change session processing state. Initiator: {initiatorId}");

      var sessionId = this.GetPrimaryKey();
      var participants = this.grainState.State.ParticipantVotes.Keys
        .Union(this.grainState.State.ModeratorIds)
        .Distinct()
        .Select(participantId => this.GrainFactory.GetGrain<IParticipantGrain>(participantId))
        .ToList();

      // Чистим участников сесии прямо тут,
      // чтобы из метода Leave не вызывался полноценный Exit текущей сессии.
      // Без этого падало InconsistentException,
      // в причинах которой разобраться не получилось,
      // больше времени на это тратить не хочется.
      this.grainState.State.ParticipantVotes.Clear();
      this.grainState.State.ModeratorIds.Clear();
      this.grainState.State.ObserverIds.Clear();

      // TODO: Нужен отдельный репозиторий сессий привязанных к участникам.
      using var _ = RequestContext.AllowCallChainReentrancy();
      await Task.WhenAll(participants.Select(participant => participant.Leave(sessionId)));

      await this.grainState.ClearStateAsync();

      this.NotifySessionRemoved();
    }

    public Task<Bulletin> GetBulletin()
    {
      return Task.FromResult(this.grainState.State.Bulletin);
    }

    public Task ChangeBulletin(Bulletin bulletine)
    {
      this.grainState.State.Bulletin = bulletine;
      var votesKeys = this.grainState.State.ParticipantVotes.Keys;
      foreach (var voteKey in votesKeys)
      {
        var vote = this.grainState.State.ParticipantVotes[voteKey];
        if (!bulletine.IsEnabled(vote))
        {
          var isSessionFinished = this.grainState.State.ProcessingState == SessionProcessingState.Finished;
          this.grainState.State.ParticipantVotes[voteKey] = isSessionFinished ? Vote.IDontKnown : Vote.Unset;
        }
      }

      this.NotifySessionInfoChanged();

      return this.grainState.WriteStateAsync();
    }

    #endregion

    #region Базовый класс

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
      if (this.grainState.State.ParticipantVotes == null)
        this.grainState.State.ParticipantVotes = new Dictionary<Guid, Vote>();

      if (this.grainState.State.ModeratorIds == null)
        this.grainState.State.ModeratorIds = new HashSet<Guid>();

      if (this.grainState.State.ObserverIds == null)
        this.grainState.State.ObserverIds = new HashSet<Guid>();

      if (this.grainState.State.Bulletin == null)
        this.grainState.State.Bulletin = Bulletin.Default();

      return base.OnActivateAsync(cancellationToken);
    }

    #endregion

    #region Методы

    private void NotifyProcessingStateChanged(SessionProcessingState newProcessingState)
    {
      var sessionId = this.GetPrimaryKey();
      var streamId = StreamId.Create(typeof(SessionProcessingNotification).FullName, sessionId);
      this.GetStreamProvider("SMS")
        .GetStream<SessionProcessingNotification>(streamId)
        .OnNextAsync(new SessionProcessingNotification(sessionId, newProcessingState))
        .Ignore();
    }

    private void NotifyNewParticipantEntered(Guid participantId)
    {
      var sessionId = this.GetPrimaryKey();
      var streamId = StreamId.Create(typeof(ParticipantsChangedNotification).FullName, sessionId);
      this.GetStreamProvider("SMS")
        .GetStream<ParticipantsChangedNotification>(streamId)
        .OnNextAsync(ParticipantsChangedNotification.CreateForChangedParticipants(sessionId, new HashSet<Guid>() { participantId }, new HashSet<Guid>()))
        .Ignore();
    }

    private void NotifyParticipantExit(Guid participantId)
    {
      var sessionId = this.GetPrimaryKey();
      var streamId = StreamId.Create(typeof(ParticipantsChangedNotification).FullName, sessionId);
      this.GetStreamProvider("SMS")
        .GetStream<ParticipantsChangedNotification>(streamId)
        .OnNextAsync(ParticipantsChangedNotification.CreateForChangedParticipants(sessionId, new HashSet<Guid>(), new HashSet<Guid>() { participantId }))
        .Ignore();
    }

    private void NotifyModeratorsChanged(HashSet<Guid> addedModerators, HashSet<Guid> removedModerator)
    {
      var sessionId = this.GetPrimaryKey();
      var streamId = StreamId.Create(typeof(ParticipantsChangedNotification).FullName, sessionId);
      this.GetStreamProvider("SMS")
        .GetStream<ParticipantsChangedNotification>(streamId)
        .OnNextAsync(ParticipantsChangedNotification.CreateForChangedModerators(sessionId, addedModerators, removedModerator))
        .Ignore();
    }

    private void NotifyObserversChanged(HashSet<Guid> addedObservers, HashSet<Guid> removedObservers)
    {
      var sessionId = this.GetPrimaryKey();
      var streamId = StreamId.Create(typeof(ParticipantsChangedNotification).FullName, sessionId);
      this.GetStreamProvider("SMS")
        .GetStream<ParticipantsChangedNotification>(streamId)
        .OnNextAsync(ParticipantsChangedNotification.CreateForChangedObservers(sessionId, addedObservers, removedObservers))
        .Ignore();
    }

    private void NotifyAcceptVote(Guid participantId, Vote vote)
    {
      var sessionId = this.GetPrimaryKey();
      var streamId = StreamId.Create(typeof(VoteNotification).FullName, sessionId);
      this.GetStreamProvider("SMS")
        .GetStream<VoteNotification>(streamId)
        .OnNextAsync(new VoteNotification(sessionId, participantId, vote))
        .Ignore();
    }

    private void NotifySessionInfoChanged()
    {
      var sessionId = this.GetPrimaryKey();
      var streamId = StreamId.Create(typeof(SessionInfoChangedNotification).FullName, sessionId);
      this.GetStreamProvider("SMS")
        .GetStream<SessionInfoChangedNotification>(streamId)
        .OnNextAsync(new SessionInfoChangedNotification(sessionId))
        .Ignore();
    }

    private void NotifySessionRemoved()
    {
      var sessionId = this.GetPrimaryKey();
      var streamId = StreamId.Create(typeof(SessionRemovedNotification).FullName, sessionId);
      this.GetStreamProvider("SMS")
        .GetStream<SessionRemovedNotification>(streamId)
        .OnNextAsync(new SessionRemovedNotification(sessionId))
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
      if (this.grainState.RecordExists)
      {
        var hasModerators = this.grainState.State.ModeratorIds.Any();
        var hasParticipants = this.grainState.State.ParticipantVotes.Any();
        return new SessionInfo()
        {
          Id = this.GetPrimaryKey(),
          // Косвенно определяем, что сессии существует по наличию в ней модераторов.
          // Ситуации, когда в сессии нет модераторов быть не может для живой сессии,
          //   иначе считаем такую сессию не инициализированной.
          IsInitialized = hasModerators,
          ModeratorIds = hasModerators ? this.grainState.State.ModeratorIds.ToImmutableArray() : ImmutableArray<Guid>.Empty,
          ObserverIds = this.grainState.State.ObserverIds.ToImmutableArray(),
          Name = this.grainState.State.Name,
          AutoStop = this.grainState.State.AutoStop,
          ProcessingState = this.grainState.State.ProcessingState,
          ParticipantsCount = hasParticipants ? this.grainState.State.ParticipantVotes.Keys.Count : 0
        };
      }

      return new SessionInfo()
      {
        IsInitialized = false
      };
    }

    /// <summary>
    /// Попытаться автоматически оставновить сессию.
    /// </summary>
    private void TryAutostopSession()
    {
      var processingState = this.grainState.State.ProcessingState;
      var observerIds = this.grainState.State.ObserverIds;
      var allParticipantsVoted = this.grainState.State.ParticipantVotes
        .Where(v => !observerIds.Contains(v.Key))
        .All(v => !Vote.Unset.Equals(v.Value));
      var sessionNotStopped = processingState != SessionProcessingState.Stopped;
      var autostopEnabled = this.grainState.State.AutoStop;
      if (autostopEnabled && allParticipantsVoted && sessionNotStopped)
      {
        this.grainState.State.ProcessingState = SessionProcessingState.Stopped;
        this.NotifyProcessingStateChanged(this.grainState.State.ProcessingState);
      }
    }

    #endregion
  }

  /// <summary>
  /// Состояние сессии.
  /// </summary>
  public class SessionGrainState
  {
    /// <summary>
    /// Получить или установить имя сессии.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Получить или установить признак необходимости автоматически остановить голосование, когда все участники проголосовали.
    /// </summary>
    public bool AutoStop { get; set; }

    /// <summary>
    /// Получить или установить список модераторов сессии.
    /// </summary>
    public HashSet<Guid> ModeratorIds { get; set; }

    /// <summary>
    /// Получить или установить список наблюдателей сессии.
    /// </summary>
    public HashSet<Guid> ObserverIds { get; set; }

    /// <summary>
    /// Получить или установить состояние голосования.
    /// </summary>
    public SessionProcessingState ProcessingState { get; set; }

    /// <summary>
    /// Получить или установить состояние голосов.
    /// </summary>
    public Dictionary<Guid, Vote> ParticipantVotes { get; set; }

    /// <summary>
    /// Получить или установить бюллетень голосования.
    /// </summary>
    public Bulletin Bulletin { get; set; }
  }
}
