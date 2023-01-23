using System;
using Orleans.Concurrency;

namespace Yotalab.PlanningPoker.Grains.Interfaces.Models.Notifications
{
  [Immutable]
  public class UserOnlineStatusChanged
  {
    public UserOnlineStatusChanged(Guid userId, bool isOnline)
    {
      this.UserId = userId;
      this.IsOnline = isOnline;
    }

    public Guid UserId { get; }

    public bool IsOnline { get; }
  }
}
