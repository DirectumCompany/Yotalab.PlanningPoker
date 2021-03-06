using Orleans.Concurrency;
using System;

namespace Yotalab.PlanningPoker.Grains.Interfaces.Models.Notifications
{
  /// <summary>
  /// Уведомление об изменении состояния сессии планирования.
  /// </summary>
  [Immutable]
  public class SessionInfoChangedNotification
  {
    public SessionInfoChangedNotification(Guid sessionId)
    {
      this.SessionId = sessionId;
    }

    /// <summary>
    /// Идентификатор сессии.
    /// </summary>
    public Guid SessionId { get; }
  }
}
