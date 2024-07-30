using System;
using Orleans;

namespace Yotalab.PlanningPoker.Grains.Interfaces.Models.Notifications
{
  [Immutable]
  [GenerateSerializer]
  public class UserOnlineStatusChanged
  {
    public UserOnlineStatusChanged(Guid userId, bool isOnline)
    {
      this.UserId = userId;
      this.IsOnline = isOnline;
    }

    [Id(0)]
    public Guid UserId { get; }

    [Id(1)]
    public bool IsOnline { get; }
  }
}
