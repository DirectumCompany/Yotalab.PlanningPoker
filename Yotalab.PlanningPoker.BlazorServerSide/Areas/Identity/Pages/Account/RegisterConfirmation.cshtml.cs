using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Pages.Account
{
  [AllowAnonymous]
  public class RegisterConfirmationModel : PageModel
  {
    private readonly UserManager<IdentityUser> userManager;

    public RegisterConfirmationModel(UserManager<IdentityUser> userManager)
    {
      this.userManager = userManager;
    }

    public string Email { get; set; }

    public async Task<IActionResult> OnGetAsync(string email)
    {
      if (email == null)
        return this.Redirect("/");

      var user = await userManager.FindByEmailAsync(email);
      if (user == null)
      {
        return NotFound($"Unable to load user with email '{email}'.");
      }

      this.Email = email;
      return Page();
    }
  }
}
