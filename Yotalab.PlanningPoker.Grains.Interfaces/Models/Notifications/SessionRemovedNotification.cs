using System;
using Orleans;
using Orleans.Concurrency;

namespace Yotalab.PlanningPoker.Grains.Interfaces.Models.Notifications
{
  [Immutable]
  [GenerateSerializer]
  public class SessionRemovedNotification
  {
    public SessionRemovedNotification(Guid sessionId)
    {
      this.SessionId = sessionId;
    }

    /// <summary>
    /// Получить идентификатор сессии.
    /// </summary>
    [Id(0)]
    public Guid SessionId { get; }
  }
}
