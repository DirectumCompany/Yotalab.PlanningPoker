using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;

namespace Yotalab.PlanningPoker.BlazorServerSide.Services
{
  public class DefaultClusterClientConnectionRetryFilter : IClientConnectionRetryFilter
  {
    private readonly ILogger<DefaultClusterClientConnectionRetryFilter> logger;

    public DefaultClusterClientConnectionRetryFilter(ILogger<DefaultClusterClientConnectionRetryFilter> logger)
    {
      this.logger = logger;
    }

    public async Task<bool> ShouldRetryConnectionAttempt(Exception exception, CancellationToken cancellationToken)
    {
      this.logger.LogError(exception, "Connect to cluster failed. Will be retry");
      await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
      return true;
    }
  }
}
