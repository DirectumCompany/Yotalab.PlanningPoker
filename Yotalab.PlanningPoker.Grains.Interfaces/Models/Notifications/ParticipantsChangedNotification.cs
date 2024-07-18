using System;
using System.Collections.Generic;
using Orleans;

namespace Yotalab.PlanningPoker.Grains.Interfaces.Models.Notifications
{
  /// <summary>
  /// Уведомление об изменении участников сессии планирования.
  /// </summary>
  [Immutable]
  [GenerateSerializer]
  public class ParticipantsChangedNotification
  {
    public static ParticipantsChangedNotification CreateForChangedParticipants(Guid sessionId, HashSet<Guid> newParticipants, HashSet<Guid> excludedParticipants)
    {
      return new ParticipantsChangedNotification(sessionId)
      {
        NewParticipants = newParticipants,
        ExcludedParticipants = excludedParticipants
      };
    }

    public static ParticipantsChangedNotification CreateForChangedModerators(Guid sessionId, HashSet<Guid> addedModerators, HashSet<Guid> removedModerators)
    {
      return new ParticipantsChangedNotification(sessionId)
      {
        AddedModerators = addedModerators,
        RemovedModerators = removedModerators
      };
    }

    public static ParticipantsChangedNotification CreateForChangedObservers(Guid sessionId, HashSet<Guid> addedObservers, HashSet<Guid> removedObservers)
    {
      return new ParticipantsChangedNotification(sessionId)
      {
        AddedObservers = addedObservers,
        RemovedObservers = removedObservers
      };
    }

    private ParticipantsChangedNotification(Guid sessionId)
    {
      this.SessionId = sessionId;
    }

    /// <summary>
    /// Получить идентификатор сессии планирования.
    /// </summary>
    [Id(0)]
    public Guid SessionId { get; }

    /// <summary>
    /// Получить новых участников планирования.
    /// </summary>
    [Id(1)]
    public HashSet<Guid> NewParticipants { get; private set; }

    /// <summary>
    /// Получить исключенных участников сессии планирования.
    /// </summary>
    [Id(2)]
    public HashSet<Guid> ExcludedParticipants { get; private set; }

    /// <summary>
    /// Получить добавленных модераторов.
    /// </summary>
    [Id(3)]
    public HashSet<Guid> AddedModerators { get; private set; }

    /// <summary>
    /// Получить удаленных модераторов.
    /// </summary>
    [Id(4)]
    public HashSet<Guid> RemovedModerators { get; private set; }

    /// <summary>
    /// Получить добавленных наблюдателей.
    /// </summary>
    [Id(5)]
    public HashSet<Guid> AddedObservers { get; private set; }

    /// <summary>
    /// Получить удаленных наблюдателей.
    /// </summary>
    [Id(6)]
    public HashSet<Guid> RemovedObservers { get; private set; }
  }
}
