using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans.Configuration;
using Orleans.Hosting;
using Yotalab.PlanningPoker.BlazorServerSide.Services;

namespace Yotalab.PlanningPoker.BlazorServerSide
{
  public static class ClusterServiceBuilderExtensions
  {
    public static IHostBuilder UseOrleansOutOfProcess(this IHostBuilder builder)
    {
      return builder
        .UseOrleansClient((context, clientBuilder) =>
        {
          var configuration = context.Configuration;
          var clusterConnectionString = configuration.GetConnectionString("DefaultClusterStorage");
          var clusterId = configuration.GetValue("Orleans:ClusterId", "planingpoker-cluster");
          var serviceId = configuration.GetValue("Orleans:ServiceId", "planingpoker");

          clientBuilder.UseAdoNetClustering(options =>
          {
            options.Invariant = "MySql.Data.MySqlConnector";
            options.ConnectionString = clusterConnectionString;
          })
          .Configure<ClusterOptions>(options =>
          {
            options.ClusterId = clusterId;
            options.ServiceId = serviceId;
          })
          .UseConnectionRetryFilter<DefaultClusterClientConnectionRetryFilter>();

          clientBuilder.AddMemoryStreams("SMS");
        });
    }
  }
}
