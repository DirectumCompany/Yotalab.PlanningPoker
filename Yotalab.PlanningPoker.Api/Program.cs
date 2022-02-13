using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Hosting;
using Yotalab.PlanningPoker.Hosting;

Console.Title = "PlanningPoker API";

await Host.CreateDefaultBuilder()
  .ConfigureAppConfiguration((context, configurationBuilder) =>
  {
    configurationBuilder.AddEnvironmentVariables("PLANNING_POKER_");
  })
  .UseOrleansSiloInProcess()
  .ConfigureServices(services =>
  {
    services.Configure<ConsoleLifetimeOptions>(options =>
    {
      options.SuppressStatusMessages = true;
    });
  })
  .RunConsoleAsync();
