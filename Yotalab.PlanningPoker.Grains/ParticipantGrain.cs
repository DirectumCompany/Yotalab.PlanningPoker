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
  ///  Грейн представляет собой участника сессии планирования.
  /// </summary>
  public class ParticipantGrain : Grain, IParticipantGrain
  {
    /// <summary>
    /// Состяние грейна.
    /// </summary>
    private IPersistentState<ParticipantGrainState> grainState;

    /// <summary>
    /// Создать экземпляр грейна.
    /// </summary>
    /// <param name="grainState">Состояние грейна.</param>
    public ParticipantGrain([PersistentState("Participant")] IPersistentState<ParticipantGrainState> grainState)
    {
      this.grainState = grainState;
    }

    #region IParticipantGrain    

    public Task<ParticipantInfo> GetAsync()
    {
      return Task.FromResult(this.ToInfo());
    }

    public async Task Join(Guid sessionId)
    {
      if (this.grainState.State.SessionIds.Contains(sessionId))
        return;

      var sessionGrain = this.GrainFactory.GetGrain<ISessionGrain>(sessionId);
      await sessionGrain.Enter(this.GetPrimaryKey());

      this.grainState.State.SessionIds.Add(sessionId);
    }

    public Task<ImmutableArray<ISessionGrain>> Sessions()
    {
      var sessionsGrains = this.grainState.State.SessionIds.Select(s => this.GrainFactory.GetGrain<ISessionGrain>(s));
      return Task.FromResult(sessionsGrains.ToImmutableArray());
    }

    public Task Vote(Guid sessionId, Vote vote)
    {
      if (!this.grainState.State.SessionIds.Contains(sessionId))
        throw new InvalidOperationException("Cannot vote within session that participant not entered");

      var sessionGrain = this.GrainFactory.GetGrain<ISessionGrain>(sessionId);
      return sessionGrain.AcceptVote(this.GetPrimaryKey(), vote);
    }

    public Task ChangeInfo(string newName, string newAvatarUrl)
    {
      this.grainState.State.Name = newName;
      this.grainState.State.AvatarUrl = newAvatarUrl;

      this.GetStreamProvider("SMS").GetStream<ParticipantChangedNotification>(Guid.Empty, typeof(IParticipantGrain).FullName)
        .OnNextAsync(new ParticipantChangedNotification(this.ToInfo()))
        .Ignore();

      return this.grainState.WriteStateAsync();
    }

    #endregion

    #region Базовый класс

    public override Task OnActivateAsync()
    {
      if (this.grainState.State.SessionIds == null)
        this.grainState.State.SessionIds = new HashSet<Guid>();

      return base.OnActivateAsync();
    }

    #endregion

    #region Методы

    private ParticipantInfo ToInfo()
    {
      return new ParticipantInfo()
      {
        Id = this.GetPrimaryKey(),
        AvatarUrl = this.grainState.State.AvatarUrl,
        Name = this.grainState.State.Name
      };
    }

    #endregion
  }

  /// <summary>
  /// Состояние грейна участника сессии планирования.
  /// </summary>
  public class ParticipantGrainState
  {
    /// <summary>
    /// Получить или установить имя участника.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Получить или установить URL до аватарки.
    /// </summary>
    public string AvatarUrl { get; set; }

    /// <summary>
    /// Получить или установить сессии, в которых состоит участник.
    /// </summary>
    public HashSet<Guid> SessionIds { get; set; }
  }
}
