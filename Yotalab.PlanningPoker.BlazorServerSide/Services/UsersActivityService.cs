using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using Orleans.Streams;
using Yotalab.PlanningPoker.Grains.Interfaces;
using Yotalab.PlanningPoker.Grains.Interfaces.Models.Notifications;

namespace Yotalab.PlanningPoker.BlazorServerSide.Services
{
  public class UsersActivityService
  {
    private readonly IClusterClient client;
    private readonly ILogger<UsersActivityService> logger;

    public UsersActivityService(IClusterClient client, ILogger<UsersActivityService> logger)
    {
      this.client = client;
      this.logger = logger;
    }

    public Task SetOnline(Guid userId, string clientId)
    {
      var userOnlineStatus = this.client.GetGrain<IUserOnlineStatusGrain>(userId);
      return userOnlineStatus.Online(clientId);
    }

    public Task SetOffline(Guid userId, string clientId)
    {
      var userOnlineStatus = this.client.GetGrain<IUserOnlineStatusGrain>(userId);
      return userOnlineStatus.Offline(clientId);
    }

    public Task<bool> GetOnlineStatus(Guid userId)
    {
      var userOnlineStatus = this.client.GetGrain<IUserOnlineStatusGrain>(userId);
      return userOnlineStatus.IsOnline();
    }

    public Task<StreamSubscriptionHandle<UserOnlineStatusChanged>> SubscribeAsync(Guid userId, Func<UserOnlineStatusChanged, Task> action) =>
        this.client.GetStreamProvider("SMS")
            .GetStream<UserOnlineStatusChanged>(StreamId.Create(typeof(UserOnlineStatusChanged).FullName, userId))
            .SubscribeAsync(new NotificationsObserver<UserOnlineStatusChanged>(this.logger, action));
  }
}
