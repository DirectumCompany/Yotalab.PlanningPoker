using System;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Data;
using Yotalab.PlanningPoker.BlazorServerSide.Services;
using Yotalab.PlanningPoker.BlazorServerSide.Services.Mailing;

namespace Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Controllers
{
  [Authorize]
  [Route("identity/[controller]")]
  [ApiController]
  public class AccountController : ControllerBase
  {
    private readonly ILogger<AccountController> logger;

    public SignInManager<IdentityUser> signInManager { get; }

    private readonly UserManager<IdentityUser> userManager;
    private readonly IEmailSender emailSender;
    private readonly ParticipantsService participantsService;

    public AccountController(
      ILogger<AccountController> logger,
      SignInManager<IdentityUser> signInManager,
      UserManager<IdentityUser> userManager,
      IEmailSender emailSender,
      ParticipantsService participantsService)
    {
      this.logger = logger;
      this.signInManager = signInManager;
      this.userManager = userManager;
      this.emailSender = emailSender;
      this.participantsService = participantsService;
    }

    [HttpPost]
    [Route("SignIn")]
    [AllowAnonymous]
    public async Task<IActionResult> SignIn([FromForm] LoginInputModel inputModel)
    {
      var user = await this.userManager.FindByEmailAsync(inputModel.Email);
      if (user != null)
      {
        var result = await this.signInManager.PasswordSignInAsync(user, inputModel.Password, inputModel.RememberMe, lockoutOnFailure: true);
        if (result.Succeeded)
        {
          this.logger.LogInformation($"User {user.Id} logged in");
          return new LoginSignedInResult();
        }

        if (result.IsLockedOut || result.IsNotAllowed)
        {
          this.logger.LogInformation("Forbidden");
          return new LoginForbiddenResult(result);
        }
      }

      this.logger.LogInformation("Unauthorized");
      return this.Unauthorized();
    }

    [HttpPost]
    [Route("SignUp")]
    [AllowAnonymous]
    public async Task<IActionResult> SignUp([FromForm] RegisterInputModel inputModel)
    {
      var user = new IdentityUser { UserName = inputModel.Email, Email = inputModel.Email };
      var result = await userManager.CreateAsync(user, inputModel.Password);
      if (result.Succeeded)
      {
        logger.LogInformation("User created a new account with password.");

        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = this.Url.ActionLink(
            "confirm",
            "account",
            values: new { userId = user.Id, code = code },
            protocol: Request.Scheme);

        await emailSender.SendEmailAsync(inputModel.Email, "Подтверждение регистрации",
            $"Подвердите вашу почту пройдя по <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>ссылке</a>.");

        if (this.userManager.Options.SignIn.RequireConfirmedAccount)
        {
          return new LoginRegisteredResult(true);
        }
        else
        {
          await signInManager.SignInAsync(user, isPersistent: false);
          return new LoginRegisteredResult(false);
        }
      }

      return new LoginRegisterFailedResult(result);
    }

    [HttpPost]
    [Route("SignOut")]
    public async Task<IActionResult> Exit()
    {
      if (this.signInManager.IsSignedIn(this.User))
        await this.signInManager.SignOutAsync();

      return this.Redirect("~/");
    }

    [HttpGet]
    [Route("confirm")]
    [AllowAnonymous]
    public async Task<IActionResult> Confirm(string userId, string code)
    {
      var user = await userManager.FindByIdAsync(userId);
      if (user != null)
      {
        code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        var result = await userManager.ConfirmEmailAsync(user, code);
        return result.Succeeded ?
          this.Redirect("~/identity/confirm?result=success") :
          this.Redirect("~/identity/confirm?result=failed");
      }

      return this.Redirect("~/identity/confirm?result=notFound");
    }

    [HttpPost]
    [Route("signInExternal")]
    [AllowAnonymous]
    public IActionResult SignInExternal([FromForm] string provider, string returnUrl = null)
    {
      var redirectUrl = this.Url.ActionLink("confirmExternal", "account", values: new { returnUrl });
      var properties = this.signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
      return this.Challenge(properties, provider);
    }

    [HttpGet]
    [Route("confirmExternal")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmExternal(string returnUrl = null, string remoteError = null)
    {
      var redirectUrl = returnUrl ?? this.Url.Content("~/");
      if (remoteError != null)
      {
        // this.ErrorMessage = $"Ошибка входа с помощью внешнего аккаунта: {remoteError}";
        return RedirectToPage("Identity/Login", new { ReturnUrl = returnUrl });
      }

      var info = await signInManager.GetExternalLoginInfoAsync();
      if (info == null)
      {
        // this.ErrorMessage = "Ошибка получения информации об аккаунте.";
        return RedirectToPage("Identity/Login", new { ReturnUrl = returnUrl });
      }

      // Sign in the user with this external login provider if the user already has a login.
      var signInResult = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
      if (signInResult.Succeeded)
      {
        logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
        return this.LocalRedirect(redirectUrl);
      }

      if (signInResult.IsLockedOut)
        return RedirectToPage("./Lockout");

      if (!info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
      {
        // this.ErrorMessage = "Ошибка получения информации об аккаунте.";
        return RedirectToPage("Identity/Login", new { ReturnUrl = returnUrl });
      }
      
      if (this.participantsService == null)
      {
        // this.ErrorMessage = "Ошибка входа в систему.";
        return RedirectToPage("Identity/Login", new { ReturnUrl = returnUrl });
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

          return this.LocalRedirect(redirectUrl);
        }
      }

      foreach (var error in identityResult.Errors)
        logger.LogError(error.Description);

      // this.ErrorMessage = string.Join(Environment.NewLine, identityResult.Errors.Select(e => e.Description));
      return RedirectToPage("Identity/Login", new { ReturnUrl = returnUrl });
    }
  }
}
