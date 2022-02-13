using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Yotalab.PlanningPoker.BlazorServerSide.Data;

[assembly: HostingStartup(typeof(Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.IdentityHostingStartup))]
namespace Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity
{
  public class IdentityHostingStartup : IHostingStartup
  {
    public void Configure(IWebHostBuilder builder)
    {
      builder.ConfigureServices((context, services) =>
      {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
          var connectionString = context.Configuration.GetConnectionString("DefaultConnection");
          options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        });

        services
          .AddIdentityCore<IdentityUser>(options =>
          {
            options.Password.RequireNonAlphanumeric = false;
            options.SignIn.RequireConfirmedAccount = true;
          })
          .AddRoles<IdentityRole>()
          .AddErrorDescriber<OverrideIdentityErrorDescriber>()
          .AddEntityFrameworkStores<ApplicationDbContext>()
          .AddSignInManager()
          .AddDefaultTokenProviders();

        services.AddAuthentication(options =>
        {
          options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
          options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
          options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        })
        .TryConfigureMicrosoftAccount(context.Configuration)
        .TryConfigureGoogleAccount(context.Configuration)
        .AddIdentityCookies();

        services.ConfigureApplicationCookie(options =>
        {
          options.AccessDeniedPath = string.Empty;
          options.ExpireTimeSpan = TimeSpan.FromDays(365 / 2); // Пол года.
          options.Events = new CookieAuthenticationEvents()
          {
            // Базовая реализация делает редирект на Login страницу.
            // За результат аутентификации в приложении отвечает API контроллер, см. LoginForbidResult.
            // Редирект делать не нужно, его при необходимости сделает Blazor приложение.
            OnRedirectToAccessDenied = (context) => Task.CompletedTask,
          };
        });

        var directory = context.Configuration.GetValue<string>("DataProtection:Directory");
        var certificateThumbprint = context.Configuration.GetValue<string>("DataProtection:CertificateThumbprint");
        var dataProtectionBuilder = services.AddDataProtection();
        if (!string.IsNullOrWhiteSpace(directory))
          dataProtectionBuilder.PersistKeysToFileSystem(new DirectoryInfo(directory));

        if (!string.IsNullOrWhiteSpace(certificateThumbprint))
          dataProtectionBuilder.ProtectKeysWithCertificate(certificateThumbprint);

        services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();
      });
    }
  }
}