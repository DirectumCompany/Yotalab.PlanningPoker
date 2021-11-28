using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Streams;

namespace Yotalab.PlanningPoker.BlazorServerSide.Services
{
  public class NotificationsObserver<T> : IAsyncObserver<T>
  {
    private readonly ILogger logger;
    private readonly Func<T, Task> action;
    private readonly CultureInfo currentCulture;
    private readonly CultureInfo currentUICulture;

    public NotificationsObserver(ILogger logger, Func<T, Task> action)
      : this(logger, action, CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture)
    {
    }

    public NotificationsObserver(ILogger logger, Func<T, Task> action, CultureInfo currentCulture, CultureInfo currentUICulture)
    {
      this.logger = logger;
      this.action = action;
      this.currentCulture = currentCulture;
      this.currentUICulture = currentUICulture;
    }

    public Task OnCompletedAsync() => Task.CompletedTask;

    public Task OnErrorAsync(Exception ex)
    {
      logger.LogError(ex, ex.Message);
      return Task.CompletedTask;
    }

    public Task OnNextAsync(T item, StreamSequenceToken token = null)
    {
      var threadCurrentCulture = CultureInfo.CurrentCulture;
      var threadCurrentUICulture = CultureInfo.CurrentUICulture;
      try
      {
        CultureInfo.CurrentCulture = this.currentCulture;
        CultureInfo.CurrentUICulture = this.currentUICulture;
        return this.action(item);
      }
      finally
      {
        CultureInfo.CurrentCulture = threadCurrentCulture;
        CultureInfo.CurrentUICulture = threadCurrentUICulture;
      }
    }
  }
}
