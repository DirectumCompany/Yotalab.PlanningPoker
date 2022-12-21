using System;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Serilog;
using Yotalab.PlanningPoker.Grains;
using Yotalab.PlanningPoker.Grains.Interfaces;

namespace Yotalab.PlanningPoker.Hosting
{
  public static class GenericHostExtensions
  {
    public static IHostBuilder UseOrleansSiloInProcess(this IHostBuilder hostBuilder)
    {
      return hostBuilder
        .UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration))
        .UseOrleans((context, builder) =>
        {
          builder.ConfigureApplicationParts(manager => manager
              .AddApplicationPart(typeof(SessionGrain).Assembly)
              .AddApplicationPart(typeof(ISessionGrain).Assembly));

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

          var clusterConnectionString = context.Configuration.GetConnectionString("DefaultClusterStorage");
          if (string.IsNullOrWhiteSpace(clusterConnectionString))
            builder.UseLocalhostClustering(siloPort, gatewayPort);
          else
            builder.UseAdoNetClustering(options =>
            {
              options.Invariant = "MySql.Data.MySqlConnector";
              options.ConnectionString = clusterConnectionString;
            })
            .Configure<ClusterOptions>(options =>
            {
              options.ClusterId = clusterId;
              options.ServiceId = "planingpoker";
            })
            .ConfigureEndpoints(siloPort, gatewayPort);

          builder.AddAdoNetGrainStorageAsDefault(options =>
          {
            options.Invariant = "MySql.Data.MySqlConnector";
            options.ConnectionString = context.Configuration.GetConnectionString("DefaultGrainStorage");
            options.UseJsonFormat = true;
            options.ConfigureJsonSerializerSettings = (jsonOptions) =>
            {
              jsonOptions.ConstructorHandling = Newtonsoft.Json.ConstructorHandling.AllowNonPublicDefaultConstructor;
              jsonOptions.ContractResolver = new PrivateSetterContractResolver();
              jsonOptions.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto;
            };
          });
          builder.AddAdoNetGrainStorage("PubSubStore", options =>
          {
            options.Invariant = "MySql.Data.MySqlConnector";
            options.ConnectionString = context.Configuration.GetConnectionString("DefaultPubSubStorage");
            options.UseJsonFormat = true;
          });
          builder.AddSimpleMessageStreamProvider("SMS");
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
