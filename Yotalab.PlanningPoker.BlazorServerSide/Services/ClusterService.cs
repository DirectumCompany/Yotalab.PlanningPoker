using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

namespace Yotalab.PlanningPoker.BlazorServerSide.Services
{
  public class ClusterService : IHostedService
  {
    private readonly ILogger<ClusterService> logger;

    public ClusterService(ILogger<ClusterService> logger, IConfiguration configuration)
    {
      this.logger = logger;
      var clusterConnectionString = configuration.GetConnectionString("DefaultClusterStorage");
      this.Client = new ClientBuilder()
        .UseAdoNetClustering(options =>
        {
          options.Invariant = "MySql.Data.MySqlConnector";
          options.ConnectionString = clusterConnectionString;
        })
        .Configure<ClusterOptions>(options =>
        {
          options.ClusterId = "planingpoker-cluster";
          options.ServiceId = "planingpoker";
        })
        .AddSimpleMessageStreamProvider("SMS")
        .Build();
    }

    public IClusterClient Client { get; }

    public Task StartAsync(CancellationToken cancellationToken) => this.Client.Connect(async error =>
      {
        logger.LogError(error, error.Message);
        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        return true;
      });

    public Task StopAsync(CancellationToken cancellationToken) => this.Client.Close();
  }
}
