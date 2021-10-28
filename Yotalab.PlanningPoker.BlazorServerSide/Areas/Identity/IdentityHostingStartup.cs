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
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
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
          options.UseMySql(context.Configuration.GetConnectionString("DefaultConnection"), dbOptions =>
          {
            dbOptions.ServerVersion(new Version(10, 5, 8), ServerType.MariaDb);
          });
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
          options.ExpireTimeSpan = TimeSpan.FromHours(24);
          options.Events = new CookieAuthenticationEvents()
          {
            // Базовая реализация делает редирект на Login страницу.
            // За результат аутентификации в приложении отвечает API контроллер, см. LoginForbidResult.
            // Редирект делать не нужно, его при необходимости сделает Blazor приложение.
            OnRedirectToAccessDenied = (context) => Task.CompletedTask,
          };
        });

        var dataProtectionConfig = context.Configuration.GetSection("DataProtection");
        if (dataProtectionConfig != null)
        {
          var directory = dataProtectionConfig["Directory"];
          var certificateThumbprint = dataProtectionConfig["CertificateThumbprint"];
          var dataProtectionBuilder = services.AddDataProtection();
          if (!string.IsNullOrWhiteSpace(directory))
            dataProtectionBuilder.PersistKeysToFileSystem(new DirectoryInfo(directory));

          if (!string.IsNullOrWhiteSpace(certificateThumbprint))
            dataProtectionBuilder.ProtectKeysWithCertificate(certificateThumbprint);
        }

        services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();
      });
    }
  }
}