using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Hosting;
using Yotalab.PlanningPoker.Hosting;

namespace Yotalab.PlanningPoker.Api
{
  public class Program
  {
    public static void Main(string[] args)
    {
      CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
      Host.CreateDefaultBuilder(args)
      .ConfigureWebHostDefaults(webBuilder =>
      {
        webBuilder.UseStartup<Startup>();
      })
      .ConfigureServices(services =>
      {
        services.Configure<ConsoleLifetimeOptions>(options =>
        {
          options.SuppressStatusMessages = true;
        });
      })
      .UseOrleansSiloInProcess();
  }
}
