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
  }
}
