using System;
using Orleans;
using Orleans.Concurrency;

namespace Yotalab.PlanningPoker.Grains.Interfaces.Models.Notifications
{
  /// <summary>
  /// Уведомление об изменении состояния сессии планирования.
  /// </summary>
  [Immutable]
  [GenerateSerializer]
  public class SessionInfoChangedNotification
  {
    public SessionInfoChangedNotification(Guid sessionId)
    {
      this.SessionId = sessionId;
    }

    /// <summary>
    /// Идентификатор сессии.
    /// </summary>
    [Id(0)]
    public Guid SessionId { get; }
  }
}
