using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySqlConnector;
using Orleans;
using Orleans.Hosting;
using Serilog;
using Yotalab.PlanningPoker.Api;
using Yotalab.PlanningPoker.Hosting;

Console.Title = "PlanningPoker API";

var builder = Host.CreateDefaultBuilder()
  .ConfigureAppConfiguration((context, configurationBuilder) =>
  {
    configurationBuilder.AddEnvironmentVariables("PLANNING_POKER_");
  })
  .UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));

if (args != null && args.Length == 1 && args[0] == "export")
{
  builder
    .ConfigureServices((context, services) =>
    {
      services.AddTransient(_ => new MySqlConnection(context.Configuration.GetConnectionString("DefaultGrainStorage")));
      services.AddHostedService<GrainStorageExportService>();
    });
}
else
{
  builder
    .UseOrleansSiloInProcess()
    .ConfigureServices((context, services) =>
    {
      services.Configure<ConsoleLifetimeOptions>(options =>
      {
        options.SuppressStatusMessages = true;
      });

      if (args != null && args.Length == 1 && args[0] == "import")
        services.AddHostedService<GrainStorageImportService>();
    });
}

await builder.RunConsoleAsync();
