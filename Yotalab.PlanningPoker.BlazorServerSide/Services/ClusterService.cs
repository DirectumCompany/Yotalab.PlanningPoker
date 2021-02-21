using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Hosting;

namespace Yotalab.PlanningPoker.BlazorServerSide.Services
{
  public class ClusterService : IHostedService
  {
    private readonly ILogger<ClusterService> logger;

    public ClusterService(ILogger<ClusterService> logger)
    {
      this.logger = logger;
      this.Client = new ClientBuilder()
        .UseLocalhostClustering()
        .AddSimpleMessageStreamProvider("SMS")
        .Build();
    }

    public IClusterClient Client { get; }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
      await this.Client.Connect(async error =>
      {
        logger.LogError(error, error.Message);
        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        return true;
      });
    }

    public Task StopAsync(CancellationToken cancellationToken) => this.Client.Close();
  }

  public static class ClusterServiceBuilderExtensions
  {
    public static IServiceCollection AddClusterService(this IServiceCollection services)
    {
      services.AddSingleton<ClusterService>();
      services.AddSingleton<IHostedService>(_ => _.GetService<ClusterService>());
      services.AddTransient(_ => _.GetService<ClusterService>().Client);
      return services;
    }
  }
}
