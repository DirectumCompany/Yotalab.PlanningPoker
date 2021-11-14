using System;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Hosting;
using Serilog;

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
          builder.ConfigureApplicationParts(manager => manager.AddFromApplicationBaseDirectory());

          // При локальном рамещении нет возможности настроить Orleans, чтобы он не занимал порты.
          // Issue запланированна на 4.0 версию https://github.com/dotnet/orleans/issues/7023
          // Поэтому пока вручную можно менять в конфиге на не занятый порт.
          // И выведем в консоль занятые порты, чтобы разбираться было легче.
          ListUsedTCPPort();
          var siloPort = context.Configuration.GetValue<int>("Orleans:SiloPort", 11111);
          var gatewayPort = context.Configuration.GetValue<int>("Orleans:GatewayPort", 30000);
          builder.UseLocalhostClustering(siloPort, gatewayPort);

          builder.AddAdoNetGrainStorageAsDefault(options =>
          {
            options.Invariant = "MySql.Data.MySqlConnector";
            options.ConnectionString = context.Configuration.GetConnectionString("DefaultGrainStorage");
            options.UseJsonFormat = true;
            options.ConfigureJsonSerializerSettings = (jsonOptions) =>
            {
              jsonOptions.ConstructorHandling = Newtonsoft.Json.ConstructorHandling.AllowNonPublicDefaultConstructor;
              jsonOptions.ContractResolver = new PrivateSetterContractResolver();
            };
          });
          builder.AddAdoNetGrainStorage("PubSubStore", options =>
          {
            options.Invariant = "MySql.Data.MySqlConnector";
            options.ConnectionString = context.Configuration.GetConnectionString("DefaultPubSubStorage");
            options.UseJsonFormat = true;
          });
          builder.AddSimpleMessageStreamProvider("SMS");
          /*builder.UseDashboard(options =>
          {
            options.HideTrace = true;
          });*/
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
