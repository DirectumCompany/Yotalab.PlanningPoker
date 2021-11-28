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
using Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Resources;
using Yotalab.PlanningPoker.BlazorServerSide.Services;
using Yotalab.PlanningPoker.BlazorServerSide.Services.Mailing;

namespace Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Controllers
{
  [Authorize]
  [Route("api/identity/[controller]")]
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
      // Clear the existing external cookie to ensure a clean login process
      this.SignOut(IdentityConstants.ExternalScheme);

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
          this.logger.LogInformation($"User {user.Id} forbidden");
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
      var identityResult = await userManager.CreateAsync(user, inputModel.Password);
      if (identityResult.Succeeded)
      {
        logger.LogInformation("User created a new account with password.");

        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = this.Url.ActionLink(
            "confirm",
            "account",
            values: new { userId = user.Id, code = code },
            protocol: this.Request.Scheme);

        await emailSender.SendEmailAsync(inputModel.Email, IdentityUIResources.SignUpConfirmationEmailTitle,
            string.Format(IdentityUIResources.SignUpConfirmationEmailBody, HtmlEncoder.Default.Encode(callbackUrl)));

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

      foreach (var error in identityResult.Errors)
        logger.LogError(error.Description);

      return new LoginRegisterFailedResult(identityResult);
    }

    [HttpGet]
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
      string encodedResult;
      var user = await userManager.FindByIdAsync(userId);
      if (user != null)
      {
        code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        var identityResult = await userManager.ConfirmEmailAsync(user, code);
        foreach (var error in identityResult.Errors)
          logger.LogError(error.Description);

        encodedResult = IdentityResultEncoder.Base64UrlEncode(identityResult);
        return this.Redirect($"~/identity/confirm?result={encodedResult}");
      }

      logger.LogWarning($"Confirm email skipped. User {userId} not found.");
      encodedResult = IdentityResultEncoder.Base64UrlEncode(IdentityResult.Failed());
      return this.Redirect($"~/identity/confirm?result={encodedResult}");
    }

    [HttpPost]
    [Route("signInExternal")]
    [AllowAnonymous]
    public IActionResult SignInExternal([FromForm] string provider, string returnUrl = null)
    {
      // Clear the existing external cookie to ensure a clean login process
      this.SignOut(IdentityConstants.ExternalScheme);

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
      string encodedResult = string.Empty;
      if (remoteError != null)
      {
        encodedResult = IdentityResultEncoder.Base64UrlEncode(IdentityResult.Failed(new IdentityError()
        {
          Description = string.Format(IdentityUIResources.SignInWithAttemptFailedRemoteError, remoteError)
        }));
        return this.Redirect($"~/identity/confirmExternal?result={encodedResult}&returnUrl={returnUrl}");
      }

      var info = await signInManager.GetExternalLoginInfoAsync();
      if (info == null)
      {
        encodedResult = IdentityResultEncoder.Base64UrlEncode(IdentityResult.Failed(new IdentityError()
        {
          Description = IdentityUIResources.SignInWithAttemptFailed
        }));
        return this.Redirect($"~/identity/confirmExternal?result={encodedResult}&returnUrl={returnUrl}");
      }

      // Sign in the user with this external login provider if the user already has a login.
      var signInResult = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: true, bypassTwoFactor: true);
      if (signInResult.Succeeded)
      {
        logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
        return this.LocalRedirect(redirectUrl);
      }

      if (signInResult.IsLockedOut)
      {
        encodedResult = IdentityResultEncoder.Base64UrlEncode(IdentityResult.Failed(new IdentityError()
        {
          Description = IdentityResources.UserLockedOut
        }));
        return this.Redirect($"~/identity/confirmExternal?result={encodedResult}&returnUrl={returnUrl}");
      }

      if (!info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
      {
        encodedResult = IdentityResultEncoder.Base64UrlEncode(IdentityResult.Failed(new IdentityError()
        {
          Description = IdentityUIResources.SignInWithAttemptFailedNotEnoughPermissions
        }));
        return this.Redirect($"~/identity/confirmExternal?result={encodedResult}&returnUrl={returnUrl}");
      }

      if (this.participantsService == null)
      {
        encodedResult = IdentityResultEncoder.Base64UrlEncode(IdentityResult.Failed(new IdentityError()
        {
          Description = IdentityUIResources.SignInWithAttemptFailed
        }));
        return this.Redirect($"~/identity/confirmExternal?result={encodedResult}&returnUrl={returnUrl}");
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

      encodedResult = IdentityResultEncoder.Base64UrlEncode(identityResult);
      return this.Redirect($"~/identity/confirmExternal?result={encodedResult}&returnUrl={returnUrl}");
    }

    [HttpPost]
    [Route("forgotPassword")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromForm] ForgotPasswordInputModel inputModel)
    {
      if (!string.IsNullOrWhiteSpace(inputModel.Email))
      {
        var user = await this.userManager.FindByEmailAsync(inputModel.Email);
        if (user == null || !await this.userManager.IsEmailConfirmedAsync(user))
        {
          // Don't reveal that the user does not exist or is not confirmed
          return this.Ok();
        }

        // For more information on how to enable account confirmation and password reset please
        // visit https://go.microsoft.com/fwlink/?LinkID=532713
        var code = await this.userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var uriBuilder = new UriBuilder();
        if (this.Request.Host.HasValue)
        {
          uriBuilder.Host = this.Request.Host.Host;
          if (this.Request.Host.Port != null)
            uriBuilder.Port = this.Request.Host.Port.Value;
        }

        uriBuilder.Scheme = this.Request.Scheme;
        uriBuilder.Path = this.Request.PathBase + "/identity/resetPassword";
        uriBuilder.Query = $"code={code}";
        var callbackUrl = uriBuilder.Uri.ToString(); // this.Url.ActionLink("resetPassword", "identity", values: new { code }, protocol: this.Request.Scheme);

        await emailSender.SendEmailAsync(inputModel.Email, IdentityUIResources.ResetPasswordEmailTitle,
            string.Format(IdentityUIResources.ResetPasswordEmailBody, HtmlEncoder.Default.Encode(callbackUrl)));
      }

      return this.Ok();
    }

    [HttpPost]
    [Route("resetPassword")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromForm] ResetPasswordInputModel inputModel)
    {
      var user = await this.userManager.FindByEmailAsync(inputModel.Email);
      if (user == null)
      {
        // Don't reveal that the user does not exist
        return this.Ok();
      }

      var result = await this.userManager.ResetPasswordAsync(user, inputModel.Code, inputModel.Password);
      if (result.Succeeded)
      {
        return this.Ok();
      }

      foreach (var error in result.Errors)
        logger.LogError(error.Description);

      return Ok();
    }
  }
}
