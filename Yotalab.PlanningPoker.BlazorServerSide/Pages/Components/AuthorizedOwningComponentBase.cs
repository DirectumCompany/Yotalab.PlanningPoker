using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Yotalab.PlanningPoker.BlazorServerSide.Pages.Components
{
  [Authorize]
  public abstract class AuthorizedOwningComponentBase<T> : OwningComponentBase<T>
  {
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; private set; }

    protected IdentityUser User { get; private set; }

    protected Guid ParticipantId => Guid.Parse(this.User.Id);

    protected override async Task OnInitializedAsync()
    {
      var authState = await this.AuthenticationStateTask;
      using (var scope = this.ScopedServices.CreateScope())
      {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        this.User = await userManager.GetUserAsync(authState.User);
      }
      await base.OnInitializedAsync();
    }
  }
}
