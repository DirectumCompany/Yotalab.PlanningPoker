using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Pages.Account
{
  [AllowAnonymous]
  public class ConfirmEmailModel : PageModel
  {
    private readonly UserManager<IdentityUser> userManager;
    private readonly ILogger<ConfirmEmailModel> logger;

    public ConfirmEmailModel(UserManager<IdentityUser> userManager, ILogger<ConfirmEmailModel> logger)
    {
      this.userManager = userManager;
      this.logger = logger;
    }

    [TempData]
    public string StatusMessage { get; set; }

    public IdentityResult Result { get; set; }

    public string ReturnUrl { get; set; }

    public async Task<IActionResult> OnGetAsync(string userId, string code, string returnUrl)
    {
      if (userId == null || code == null)
      {
        return RedirectToPage("/");
      }

      var user = await userManager.FindByIdAsync(userId);
      if (user == null)
      {
        return NotFound($"Unable to load user with ID '{userId}'.");
      }

      code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
      this.Result = await userManager.ConfirmEmailAsync(user, code);
      this.StatusMessage = this.Result.Succeeded ? "Спасибо, что выбрали нас!" : "Ошибка подтверждения эл. почты.";
      this.ReturnUrl = returnUrl;
      if (!this.Result.Succeeded)
      {
        this.logger.LogWarning("Confirm email ({Email}) failed: {Error}", user.Email, string.Join(";", this.Result.Errors.Select(e => e.Description)));
      }
      return Page();
    }
  }
}
