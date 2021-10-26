using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Data;
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

    public AccountController(
      ILogger<AccountController> logger,
      SignInManager<IdentityUser> signInManager,
      UserManager<IdentityUser> userManager,
      IEmailSender emailSender)
    {
      this.logger = logger;
      this.signInManager = signInManager;
      this.userManager = userManager;
      this.emailSender = emailSender;
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

        await emailSender.SendEmailAsync(inputModel.Email, "Подтвердите вашу эл. почту",
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
  }
}
