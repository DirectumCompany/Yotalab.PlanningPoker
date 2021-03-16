using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Streams;

namespace Yotalab.PlanningPoker.BlazorServerSide.Services
{
  public class NotificationsObserver<T> : IAsyncObserver<T>
  {
    private readonly ILogger logger;
    private readonly Func<T, Task> action;

    public NotificationsObserver(ILogger logger, Func<T, Task> action)
    {
      this.logger = logger;
      this.action = action;
    }

    public Task OnCompletedAsync() => Task.CompletedTask;

    public Task OnErrorAsync(Exception ex)
    {
      logger.LogError(ex, ex.Message);
      return Task.CompletedTask;
    }

    public Task OnNextAsync(T item, StreamSequenceToken token = null) => action(item);
  }
}
