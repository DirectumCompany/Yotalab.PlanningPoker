using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Yotalab.PlanningPoker.BlazorServerSide.Services;

namespace Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Pages.Account
{
  [AllowAnonymous]
  public class ExternalLoginModel : PageModel
  {
    private readonly SignInManager<IdentityUser> signInManager;
    private readonly UserManager<IdentityUser> userManager;
    private readonly ILogger<ExternalLoginModel> logger;
    private readonly ParticipantsService participantsService;

    public ExternalLoginModel(
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager,
        ILogger<ExternalLoginModel> logger,
        ParticipantsService participantsService)
    {
      this.signInManager = signInManager;
      this.userManager = userManager;
      this.logger = logger;
      this.participantsService = participantsService;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public string ProviderDisplayName { get; set; }

    public string ReturnUrl { get; set; }

    [TempData]
    public string ErrorMessage { get; set; }

    public class InputModel
    {
      [Required(ErrorMessage = "The Email field is required")]
      [EmailAddress(ErrorMessage = "The {0} field is not a valid e-mail address")]
      [Display(Name = "Email")]
      public string Email { get; set; }
    }

    public IActionResult OnGetAsync()
    {
      return RedirectToPage("/");
    }

    public IActionResult OnPost(string provider, string returnUrl = null)
    {
      // Request a redirect to the external login provider.
      var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
      var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
      return new ChallengeResult(provider, properties);
    }

    public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
    {
      returnUrl = returnUrl ?? Url.Content("~/");

      if (remoteError != null)
      {
        this.ErrorMessage = $"Ошибка входа с помощью внешнего аккаунта: {remoteError}";
        return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
      }

      var info = await signInManager.GetExternalLoginInfoAsync();
      if (info == null)
      {
        this.ErrorMessage = "Ошибка получения информации об аккаунте.";
        return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
      }

      // Sign in the user with this external login provider if the user already has a login.
      var signInResult = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
      if (signInResult.Succeeded)
      {
        logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
        return this.LocalRedirect(returnUrl);
      }

      if (signInResult.IsLockedOut)
        return RedirectToPage("./Lockout");

      // If the user does not have an account, then ask the user to create an account.
      this.ReturnUrl = returnUrl;
      this.ProviderDisplayName = info.ProviderDisplayName;
      if (!info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
      {
        this.ErrorMessage = "Ошибка получения информации об аккаунте.";
        return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
      }

      var email = info.Principal.FindFirstValue(ClaimTypes.Email);
      var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
      var identityResult = await userManager.CreateAsync(user);

      if (identityResult.Succeeded)
      {
        identityResult = await userManager.AddLoginAsync(user, info);
        if (identityResult.Succeeded)
        {
          logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);
          await signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);

          var avatarUrl = info.Principal.FindFirstValue("avatar");
          await this.participantsService.ChangeInfo(Guid.Parse(user.Id), info.Principal.Identity.Name, avatarUrl);

          return this.LocalRedirect(returnUrl);
        }
      }

      foreach (var error in identityResult.Errors)
        logger.LogError(error.Description);

      this.ErrorMessage = string.Join(Environment.NewLine, identityResult.Errors.Select(e => e.Description));
      return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
    }
  }
}
