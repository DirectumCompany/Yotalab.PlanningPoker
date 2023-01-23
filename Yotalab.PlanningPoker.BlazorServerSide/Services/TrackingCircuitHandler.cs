using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Identity;

namespace Yotalab.PlanningPoker.BlazorServerSide.Services
{
  public class TrackingCircuitHandler : CircuitHandler
  {
    private readonly AuthenticationStateProvider authenticationStateProvider;
    private readonly UserManager<IdentityUser> userManager;
    private readonly UsersActivityService userActivityService;

    public TrackingCircuitHandler(
      AuthenticationStateProvider authenticationStateProvider,
      UserManager<IdentityUser> userManager,
      UsersActivityService userActivityService)
    {
      this.authenticationStateProvider = authenticationStateProvider;
      this.userManager = userManager;
      this.userActivityService = userActivityService;
    }

    public override async Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
      await base.OnCircuitOpenedAsync(circuit, cancellationToken);

      var user = await GetCurrentUserAsync();
      if (user == null)
        return;

      await this.userActivityService.SetOnline(Guid.Parse(user.Id), circuit.Id);
    }

    public override async Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
      await base.OnCircuitClosedAsync(circuit, cancellationToken);

      var user = await GetCurrentUserAsync();
      if (user == null)
        return;

      await this.userActivityService.SetOffline(Guid.Parse(user.Id), circuit.Id);
    }

    private async Task<IdentityUser> GetCurrentUserAsync()
    {
      var authState = await this.authenticationStateProvider?.GetAuthenticationStateAsync();
      if (authState == null)
        return null;

      return await userManager.GetUserAsync(authState.User);
    }
  }
}
