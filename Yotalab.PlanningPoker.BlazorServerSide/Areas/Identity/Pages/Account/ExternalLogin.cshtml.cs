using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Yotalab.PlanningPoker.BlazorServerSide.Services;

namespace Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Pages.Account
{
  [AllowAnonymous]
  public class ExternalLoginModel : PageModel
  {
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<ExternalLoginModel> _logger;
    private readonly ParticipantsService participantsService;

    public ExternalLoginModel(
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager,
        ILogger<ExternalLoginModel> logger,
        IEmailSender emailSender,
        ParticipantsService participantsService)
    {
      _signInManager = signInManager;
      _userManager = userManager;
      _logger = logger;
      _emailSender = emailSender;
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
      [Required]
      [EmailAddress]
      public string Email { get; set; }
    }

    public IActionResult OnGetAsync()
    {
      return RedirectToPage("./Login");
    }

    public IActionResult OnPost(string provider, string returnUrl = null)
    {
      // Request a redirect to the external login provider.
      var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
      var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
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

      var info = await _signInManager.GetExternalLoginInfoAsync();
      if (info == null)
      {
        this.ErrorMessage = "Ошибка получения информации об аккаунте.";
        return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
      }

      // Sign in the user with this external login provider if the user already has a login.
      var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
      if (signInResult.Succeeded)
      {
        _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
        return LocalRedirect(returnUrl);
      }

      if (signInResult.IsLockedOut)
        return RedirectToPage("./Lockout");

      // If the user does not have an account, then ask the user to create an account.
      this.ReturnUrl = returnUrl;
      this.ProviderDisplayName = info.ProviderDisplayName;
      if (!info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
        return Page();

      var email = info.Principal.FindFirstValue(ClaimTypes.Email);
      var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
      var identityResult = await _userManager.CreateAsync(user);

      if (identityResult.Succeeded)
      {
        identityResult = await _userManager.AddLoginAsync(user, info);
        if (identityResult.Succeeded)
        {
          _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);
          await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);

          var avatarUrl = info.Principal.FindFirstValue("avatar");
          await this.participantsService.ChangeInfo(Guid.Parse(user.Id), info.Principal.Identity.Name, avatarUrl);

          return LocalRedirect(returnUrl);
        }
      }

      foreach (var error in identityResult.Errors)
        _logger.LogError(error.Description);

      this.ErrorMessage = "Ошибка входа с помощью внешнего аккаунта.";
      return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
    }

    public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
    {
      returnUrl = returnUrl ?? Url.Content("~/");
      // Get the information about the user from the external login provider
      var info = await _signInManager.GetExternalLoginInfoAsync();
      if (info == null)
      {
        ErrorMessage = "Error loading external login information during confirmation.";
        return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
      }

      if (ModelState.IsValid)
      {
        var user = new IdentityUser { UserName = Input.Email, Email = Input.Email };

        var result = await _userManager.CreateAsync(user);
        if (result.Succeeded)
        {
          result = await _userManager.AddLoginAsync(user, info);
          if (result.Succeeded)
          {
            _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = userId, code = code },
                protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(Input.Email, "Подтвердите вашу эл. почту",
              $"Подвердите вашу почту пройдя по <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>ссылке</a>.");

            // If account confirmation is required, we need to show the link if we don't have a real email sender
            if (_userManager.Options.SignIn.RequireConfirmedAccount)
            {
              return RedirectToPage("./RegisterConfirmation", new { Email = Input.Email });
            }

            await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);

            await this.participantsService.ChangeInfo(Guid.Parse(user.Id), info.Principal.Identity.Name, null);

            return LocalRedirect(returnUrl);
          }
        }
        foreach (var error in result.Errors)
        {
          ModelState.AddModelError(string.Empty, error.Description);
        }
      }

      ProviderDisplayName = info.ProviderDisplayName;
      ReturnUrl = returnUrl;
      return Page();
    }
  }
}
