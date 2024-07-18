using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using Yotalab.PlanningPoker.Grains.Interfaces;
using Yotalab.PlanningPoker.Grains.Interfaces.Models.Notifications;

namespace Yotalab.PlanningPoker.Grains
{
  public class UserOnlineStatusGrain : Grain, IUserOnlineStatusGrain
  {
    public UserOnlineStatusGrain(ILogger<UserOnlineStatusGrain> logger)
    {
      this.logger = logger;
    }

    private readonly ILogger<UserOnlineStatusGrain> logger;

    private IDisposable offlineNotificationTimer;

    private HashSet<string> clients = new HashSet<string>();

    private bool HasActiveClients => this.clients.Count > 0;

    public Task<bool> IsOnline()
    {
      return Task.FromResult(this.HasActiveClients);
    }

    public Task Offline(string clientId)
    {
      var isRemoved = this.clients.Remove(clientId);
      if (this.clients.Count == 0 && isRemoved)
      {
        this.DisposeOfflineNotificationTimer();

        // Делаем небольшую задержку, чтобы исключить ситуации быстрого обновления статуса.
        // Например, если пользователь обновил страницу, то без задержки статус успеет моргнуть.
        this.offlineNotificationTimer = this.RegisterGrainTimer(
          this.OnNotifyOfflineTimer,
          new GrainTimerCreationOptions
          {
            DueTime = TimeSpan.FromMilliseconds(500),
            Period = Timeout.InfiniteTimeSpan,
            Interleave = true
          });
      }

      return Task.CompletedTask;
    }

    public Task Online(string clientId)
    {
      var isAdded = this.clients.Add(clientId);
      if (this.clients.Count == 1 && isAdded)
      {
        // Если пользователь успел появиться в сети, то не нужно уведомлять об выходе из сети.
        var offlineNotificationTimerActive = this.offlineNotificationTimer != null;
        this.DisposeOfflineNotificationTimer();

        this.NotifyOnlineStatusChanged();

        if (!offlineNotificationTimerActive)
          this.logger.LogInformation($"User {this.GetPrimaryKey()} online");
      }

      return Task.CompletedTask;
    }

    private Task OnNotifyOfflineTimer()
    {
      this.DisposeOfflineNotificationTimer();

      this.NotifyOnlineStatusChanged();
      this.logger.LogInformation($"User {this.GetPrimaryKey()} offline");
      return Task.CompletedTask;
    }

    private void NotifyOnlineStatusChanged()
    {
      var userId = this.GetPrimaryKey();
      var streamId = StreamId.Create(typeof(UserOnlineStatusChanged).FullName, userId);
      this.GetStreamProvider("SMS")
        .GetStream<UserOnlineStatusChanged>(streamId)
        .OnNextAsync(new UserOnlineStatusChanged(userId, this.HasActiveClients))
        .Ignore();
    }

    private void DisposeOfflineNotificationTimer()
    {
      this.offlineNotificationTimer?.Dispose();
      this.offlineNotificationTimer = null;
    }
  }
}
