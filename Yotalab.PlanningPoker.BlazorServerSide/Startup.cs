using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using Yotalab.PlanningPoker.BlazorServerSide.Services;
using Yotalab.PlanningPoker.BlazorServerSide.Services.FilesStoraging;
using Yotalab.PlanningPoker.BlazorServerSide.Services.Mailing;

namespace Yotalab.PlanningPoker.BlazorServerSide
{
  public class Startup
  {
    public Startup(IHostEnvironment environment, IConfiguration configuration)
    {
      this.Environment = environment;
      this.Configuration = configuration;
    }

    public IHostEnvironment Environment { get; }
    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddRazorPages();
      services.AddServerSideBlazor();
      // Если используется UseOrleansSiloInProcess то эту строчку надо оставить закоментированной.
      // services.AddClusterService();

      services.AddLocalization();

      if (this.Environment.IsDevelopment())
      {
        services.AddTransient<IEmailSender, DebugEmailSender>();
      }
      else
      {
        services.AddTransient<IEmailSender>(context =>
        {
          return new SmtpEmailSender(
            new SmtpEmailSenderOptions(
              this.Configuration["SmtpEmailSender:Host"],
              this.Configuration.GetValue<int>("SmtpEmailSender:Port"),
              this.Configuration.GetValue<bool>("SmtpEmailSender:EnableSSL"),
              this.Configuration["SmtpEmailSender:UserName"],
              this.Configuration["SmtpEmailSender:Password"]),
             context.GetRequiredService<ILogger<SmtpEmailSender>>()
            );
        });
      }

      services.AddSingleton<SessionService>();
      services.AddSingleton<ParticipantsService>();
      services.AddScoped<JSInteropFunctions>();
      services.AddMudServices();
      services.AddHttpClient();
      services.AddSingleton(provider =>
      {
        var webEnvironment = provider.GetRequiredService<IWebHostEnvironment>();
        return new AvatarStorage(webEnvironment.WebRootPath, "img/avatars", "img/avatars/tmp");
      });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
        app.UseDatabaseErrorPage();
      }
      else
      {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
      }

      app.UseHttpsRedirection();
      app.UseStaticFiles();

      var supportedCultures = new[] { "en", "ru" };
      app.UseRequestLocalization(supportedCultures);

      app.UseRouting();

      app.UseAuthentication();
      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
        endpoints.MapBlazorHub();
        endpoints.MapFallbackToPage("/_Host");
      });
    }
  }
}
