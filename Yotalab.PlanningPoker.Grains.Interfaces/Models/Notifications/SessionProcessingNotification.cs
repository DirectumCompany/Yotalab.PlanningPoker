using System;
using Orleans;

namespace Yotalab.PlanningPoker.Grains.Interfaces.Models.Notifications
{
  /// <summary>
  /// Уведомление об изменении состояния сессии планирования.
  /// </summary>
  [Immutable]
  [GenerateSerializer]
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
    [Id(0)]
    public Guid SessionId { get; }

    /// <summary>
    /// Получить новое состояние сессии.
    /// </summary>
    [Id(1)]
    public SessionProcessingState NewProcessingState { get; }
  }
}
