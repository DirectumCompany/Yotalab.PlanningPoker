using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Yotalab.PlanningPoker.BlazorServerSide.Services;

namespace Yotalab.PlanningPoker.BlazorServerSide
{
  public static class ClusterServiceBuilderExtensions
  {
    public static IHostBuilder UseOrleansOutOfProcess(this IHostBuilder builder)
    {
      return builder
        .UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration))
        .ConfigureServices(services =>
        {
          services.AddSingleton<ClusterService>();
          services.AddSingleton<IHostedService>(_ => _.GetService<ClusterService>());
          services.AddTransient(_ => _.GetService<ClusterService>().Client);
        });
    }
  }
}
