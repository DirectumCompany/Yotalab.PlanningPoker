using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
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
          .AddDefaultIdentity<IdentityUser>(options =>
          {
            options.Password.RequireNonAlphanumeric = false;
            options.SignIn.RequireConfirmedAccount = true;
          })
          .AddErrorDescriber<OverrideIdentityErrorDescriber>()
          .AddEntityFrameworkStores<ApplicationDbContext>();

        services
          .AddAuthentication()
          .AddMicrosoftAccount(options =>
          {
            options.ClientId = context.Configuration["Authentication:Microsoft:ClientId"];
            options.ClientSecret = context.Configuration["Authentication:Microsoft:ClientSecret"];
          })
          .AddGoogle(options =>
          {
            options.ClientId = context.Configuration["Authentication:Google:ClientId"];
            options.ClientSecret = context.Configuration["Authentication:Google:ClientSecret"];
            options.Events = new Microsoft.AspNetCore.Authentication.OAuth.OAuthEvents
            {
              OnCreatingTicket = c =>
              {
                var identity = (ClaimsIdentity)c.Principal.Identity;
                var avatar = c.User.GetProperty("picture").GetString();
                identity.AddClaim(new Claim("avatar", avatar));
                return Task.FromResult(0);
              }
            };
          });

        services.ConfigureApplicationCookie(options =>
        {
          options.ExpireTimeSpan = TimeSpan.FromHours(24);
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