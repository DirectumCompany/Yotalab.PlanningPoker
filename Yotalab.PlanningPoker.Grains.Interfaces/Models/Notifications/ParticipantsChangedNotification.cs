using System;
using System.Collections.Generic;
using Orleans.Concurrency;

namespace Yotalab.PlanningPoker.Grains.Interfaces.Models.Notifications
{
  /// <summary>
  /// Уведомление об изменении участников сессии планирования.
  /// </summary>
  [Immutable]
  public class ParticipantsChangedNotification
  {
    public ParticipantsChangedNotification(Guid sessionId, HashSet<Guid> newParticipants, HashSet<Guid> excludedParticipants)
    {
      this.SessionId = sessionId;
      this.NewParticipants = newParticipants;
      this.ExcludedParticipants = excludedParticipants;
    }

    public ParticipantsChangedNotification(Guid sessionId,
      HashSet<Guid> newParticipants, HashSet<Guid> excludedParticipants,
      HashSet<Guid> addedModerators, HashSet<Guid> removedModerators)
      : this(sessionId, newParticipants, excludedParticipants)
    {
      this.AddedModerators = addedModerators;
      this.RemovedModerators = removedModerators;
    }

    /// <summary>
    /// Получить идентификатор сессии планирования.
    /// </summary>
    public Guid SessionId { get; }

    /// <summary>
    /// Получить новых участников планирования.
    /// </summary>
    public HashSet<Guid> NewParticipants { get; }

    /// <summary>
    /// Получить исключенных участников сессии планирования.
    /// </summary>
    public HashSet<Guid> ExcludedParticipants { get; }

    /// <summary>
    /// Получить добавленных модераторов.
    /// </summary>
    public HashSet<Guid> AddedModerators { get; }

    /// <summary>
    /// Получить удаленных модераторов.
    /// </summary>
    public HashSet<Guid> RemovedModerators { get; }
  }
}
