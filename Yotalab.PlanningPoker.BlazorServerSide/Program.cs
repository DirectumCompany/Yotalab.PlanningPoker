using System;
using System.Runtime;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Yotalab.PlanningPoker.BlazorServerSide.Data;
using Yotalab.PlanningPoker.Hosting;

namespace Yotalab.PlanningPoker.BlazorServerSide
{
  public class Program
  {
    public static void Main(string[] args)
    {
      var host = CreateHostBuilder(args).Build();
      using (var scope = host.Services.CreateScope())
      {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        MigrateDatabase(services, logger);
        logger.LogDebug("Is server garbage collection is {IsServerGC}", GCSettings.IsServerGC ? "enabled" : "disabled");
      }

      host.Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
      var builder = Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((context, configurationBuilder) =>
        {
          configurationBuilder.AddEnvironmentVariables("PLANNING_POKER_");
        })
        .ConfigureWebHostDefaults(webBuilder =>
        {
          webBuilder.UseStartup<Startup>();
        })
        .UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));

      var useOrleansClusterValue = Environment.GetEnvironmentVariable("PLANNING_POKER_ORLEANS__USECLUSTER");
      bool useOrleansCluster;
      if (!bool.TryParse(useOrleansClusterValue, out useOrleansCluster))
        useOrleansCluster = false;

      if (useOrleansCluster)
        builder.UseOrleansOutOfProcess();
      else
        builder.UseOrleansSiloInProcess(useOrleansCluster);

      return builder;
    }

    private static void MigrateDatabase(IServiceProvider services, ILogger<Program> logger)
    {
      try
      {
        var identityContext = services.GetRequiredService<ApplicationDbContext>();
        identityContext.Database.Migrate();

        logger.LogInformation("Database is ready.");
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "An error occurred while seeding the database.");
      }
    }
  }
}
