using System;
using Orleans.Concurrency;

namespace Yotalab.PlanningPoker.Grains.Interfaces.Models.Notifications
{
  /// <summary>
  /// Уведомление об изменении состояния сессии планирования.
  /// </summary>
  [Immutable]
  public class SessionProcessingNotification
  {
    public SessionProcessingNotification(Guid sessionId, SessionProcessingState newProcessingState)
    {
      this.SessionId = sessionId;
      this.NewProcessingState = newProcessingState;
    }

    /// <summary>
    /// Получить идентификатор сессии.
    /// </summary>
    public Guid SessionId { get; }

    /// <summary>
    /// Получить новое состояние сессии.
    /// </summary>
    public SessionProcessingState NewProcessingState { get; }
  }
}
