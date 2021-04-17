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
