using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity
{
  /// <summary>
  /// Расширения для настройки входа через сторонние провайдеры аутентификации.
  /// </summary>
  public static class AuthenticationProvidersServiceCollectionExtensions
  {
    /// <summary>
    /// Попробовать сконфигурировать вход через учетную запись Microsoft, при наличии в конфигурации приложения ClientId и ClientSecret.
    /// </summary>
    /// <param name="authencticationBuilder">Построитель аутентификации.</param>
    /// <param name="configuration">Конфигурация приложения.</param>
    /// <returns>Построитель аутентификации, сконфигурированный для входа через учетную запись Microsoft,
    /// если в конфигурации приложения указаны необходимые ключи.</returns>
    public static AuthenticationBuilder TryConfigureMicrosoftAccount(this AuthenticationBuilder authencticationBuilder, IConfiguration configuration)
    {
      var microsoftAccountClientId = configuration["Authentication:Microsoft:ClientId"];
      var microsoftAccountClientSecret = configuration["Authentication:Microsoft:ClientSecret"];
      if (string.IsNullOrWhiteSpace(microsoftAccountClientId) || string.IsNullOrWhiteSpace(microsoftAccountClientSecret))
        return authencticationBuilder;

      return authencticationBuilder.AddMicrosoftAccount(options =>
      {
        options.ClientId = microsoftAccountClientId;
        options.ClientSecret = microsoftAccountClientSecret;
        options.AccessDeniedPath = "/Login";
      });
    }

    /// <summary>
    /// Попробовать сконфигурировать вход через учетную запись Google, при наличии в конфигурации приложения ClientId и ClientSecret.
    /// </summary>
    /// <param name="authencticationBuilder">Построитель аутентификации.</param>
    /// <param name="configuration">Конфигурация приложения.</param>
    /// <returns>Построитель аутентификации, сконфигурированный для входа через учетную запись Google,
    /// если в конфигурации приложения указаны необходимые ключи.</returns>
    public static AuthenticationBuilder TryConfigureGoogleAccount(this AuthenticationBuilder authencticationBuilder, IConfiguration configuration)
    {
      var googleAccountClientId = configuration["Authentication:Google:ClientId"];
      var googleAccountClientSecret = configuration["Authentication:Google:ClientSecret"];
      if (string.IsNullOrWhiteSpace(googleAccountClientId) || string.IsNullOrWhiteSpace(googleAccountClientSecret))
        return authencticationBuilder;

      return authencticationBuilder.AddGoogle(options =>
      {
        options.ClientId = googleAccountClientId;
        options.ClientSecret = googleAccountClientSecret;
        options.Events = new Microsoft.AspNetCore.Authentication.OAuth.OAuthEvents
        {
          OnCreatingTicket = c =>
          {
            var identity = (ClaimsIdentity)c.Principal.Identity;
            var avatar = c.User.GetProperty("picture").GetString();
            identity.AddClaim(new Claim("avatar", avatar));
            return Task.CompletedTask;
          }
        };
      });
    }
  }
}
