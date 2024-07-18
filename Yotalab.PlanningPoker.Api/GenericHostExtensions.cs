using System;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Serialization;
using Orleans.Storage;

namespace Yotalab.PlanningPoker.Hosting
{
  public static class GenericHostExtensions
  {
    public static IHostBuilder UseOrleansSiloInProcess(this IHostBuilder hostBuilder)
    {
      return hostBuilder
        .UseOrleans((context, builder) =>
        {
          // При локальном рамещении нет возможности настроить Orleans, чтобы он не занимал порты.
          // Issue запланированна на 4.0 версию https://github.com/dotnet/orleans/issues/7023
          // Поэтому пока вручную можно менять в конфиге на не занятый порт.
          // И выведем в консоль занятые порты, чтобы разбираться было легче.
          ListUsedTCPPort();
          var siloPort = context.Configuration.GetValue("Orleans:SiloPort", 11111);
          var gatewayPort = context.Configuration.GetValue("Orleans:GatewayPort", 30000);
          var dashboardPort = context.Configuration.GetValue("Orleans:DashboardPort", 8080);
          var useDashboard = context.Configuration.GetValue("Orleans:UseDashboard", false);
          var dashboardHost = context.Configuration.GetValue("Orleans:DashboardHost", false);
          var clusterId = context.Configuration.GetValue("Orleans:ClusterId", "planingpoker-cluster");
          var serviceId = context.Configuration.GetValue("Orleans:ServiceId", "planingpoker");

          var clusterConnectionString = context.Configuration.GetConnectionString("DefaultClusterStorage");
          if (string.IsNullOrWhiteSpace(clusterConnectionString))
            builder.UseLocalhostClustering(siloPort, gatewayPort, serviceId: serviceId, clusterId: clusterId);
          else
            builder.UseAdoNetClustering(options =>
            {
              options.Invariant = "MySql.Data.MySqlConnector";
              options.ConnectionString = clusterConnectionString;
            })
            .Configure<ClusterOptions>(options =>
            {
              options.ClusterId = clusterId;
              options.ServiceId = serviceId;
            })
            .ConfigureEndpoints(siloPort, gatewayPort);

          builder.AddAdoNetGrainStorageAsDefault(options =>
          {
            options.Invariant = "MySql.Data.MySqlConnector";
            options.ConnectionString = context.Configuration.GetConnectionString("DefaultGrainStorage");
            options.GrainStorageSerializer = new JsonGrainStorageSerializer(
              new OrleansJsonSerializer(
                Options.Create(new OrleansJsonSerializerOptions
                {
                  JsonSerializerSettings = new Newtonsoft.Json.JsonSerializerSettings()
                  {
                    ConstructorHandling = Newtonsoft.Json.ConstructorHandling.AllowNonPublicDefaultConstructor,
                    ContractResolver = new PrivateSetterContractResolver(),
                    TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
                    DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore,
                    NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                    PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects
                  }
                })));
          });

          builder
            .AddMemoryGrainStorage("PubSubStore")
            .AddMemoryStreams("SMS");

          if (useDashboard || dashboardHost)
          {
            builder.UseDashboard(options =>
            {
              options.Port = dashboardPort;
              options.HideTrace = true;
              options.HostSelf = dashboardHost;
            });
          }
        });
    }

    private static void ListUsedTCPPort()
    {
      var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
      var tcpConnections = ipGlobalProperties.GetActiveTcpConnections();
      foreach (var connection in tcpConnections)
        Console.WriteLine("Used Port {0} {1} {2} ", connection.LocalEndPoint, connection.RemoteEndPoint, connection.State);
    }
  }
}
