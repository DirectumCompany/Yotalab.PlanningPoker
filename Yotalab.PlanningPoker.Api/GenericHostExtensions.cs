using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Hosting;
using Serilog;
using Serilog.Events;

namespace Yotalab.PlanningPoker.Hosting
{
  public static class GenericHostExtensions
  {
    public static IHostBuilder UseOrleansSiloInProcess(this IHostBuilder hostBuilder)
    {
      return hostBuilder
        .ConfigureLogging((context, builder) =>
        {
          builder.AddFilter("Orleans.Runtime.Management.ManagementGrain", LogLevel.Warning);
          builder.AddFilter("Orleans.Runtime.SiloControl", LogLevel.Warning);
          builder.AddSerilog(new LoggerConfiguration()
              .ReadFrom.Configuration(context.Configuration)
              .Filter.ByExcluding(e => e.Properties["SourceContext"].ToString() == @"""Orleans.Runtime.Management.ManagementGrain""" && e.Level < LogEventLevel.Warning)
              .Filter.ByExcluding(e => e.Properties["SourceContext"].ToString() == @"""Orleans.Runtime.SiloControl""" && e.Level < LogEventLevel.Warning)
              .CreateLogger());
        })
        .UseOrleans((context, builder) =>
        {
          builder.ConfigureApplicationParts(manager => manager.AddFromApplicationBaseDirectory());
          builder.UseLocalhostClustering();
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
  }
}
