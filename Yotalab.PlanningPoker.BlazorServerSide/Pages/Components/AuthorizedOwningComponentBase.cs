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

    [Inject]
    private NavigationManager Navigation { get; set; }

    protected IdentityUser User { get; private set; }

    protected Guid ParticipantId => this.User != null ? Guid.Parse(this.User.Id) : Guid.Empty;

    protected override async Task OnInitializedAsync()
    {
      var authState = await this.AuthenticationStateTask;
      using (var scope = this.ScopedServices.CreateScope())
      {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        this.User = await userManager.GetUserAsync(authState.User);
        if (this.User == null)
          this.Navigation.NavigateTo("api/identity/account/signOut", true);
      }
      await base.OnInitializedAsync();
    }
  }
}
