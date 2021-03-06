using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Radzen;
using Yotalab.PlanningPoker.BlazorServerSide.Services;
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
      services.AddRazorPages()
        .AddDataAnnotationsLocalization(options =>
          options.DataAnnotationLocalizerProvider = CreateDataAnnotationLocalizer);
      services.AddServerSideBlazor();
      // ���� ������������ UseOrleansSiloInProcess �� ��� ������� ���� �������� �����������������.
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

      services.AddScoped<NotificationService>();
      services.AddSingleton<SessionService>();
      services.AddSingleton<ParticipantsService>();
      services.AddScoped<JSInteropFunctions>();
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

      var supportedCultures = new[] { "en-US", "ru-RU" };
      app.UseRequestLocalization(new RequestLocalizationOptions()
        .SetDefaultCulture(supportedCultures[1])
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures));

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

    /// <summary>
    /// ��������� ����� ��������� ������������ ��� DataAnnotaion ���������.
    /// </summary>
    /// <param name="type">��� ������.</param>
    /// <param name="factory">������� �������������.</param>
    /// <returns>����������� �����.</returns>
    private static IStringLocalizer CreateDataAnnotationLocalizer(Type type, IStringLocalizerFactory factory)
    {
      if (type.FullName.StartsWith("Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Pages.Account"))
        return factory.Create(typeof(Resources.IdentityResource));

      return factory.Create(type);
    }
  }
}
