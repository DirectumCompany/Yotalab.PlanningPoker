using System;
using Orleans.Concurrency;

namespace Yotalab.PlanningPoker.Grains.Interfaces.Models.Notifications
{
  [Immutable]
  public class SessionRemovedNotification
  {
    public SessionRemovedNotification(Guid sessionId)
    {
      this.SessionId = sessionId;
    }

    /// <summary>
    /// Получить идентификатор сессии.
    /// </summary>
    public Guid SessionId { get; }
  }
}
