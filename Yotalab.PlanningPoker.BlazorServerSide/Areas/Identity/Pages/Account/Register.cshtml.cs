using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Yotalab.PlanningPoker.BlazorServerSide.Services.Mailing;

namespace Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Pages.Account
{
  [AllowAnonymous]
  public class RegisterModel : PageModel
  {
    private readonly SignInManager<IdentityUser> signInManager;
    private readonly UserManager<IdentityUser> userManager;
    private readonly ILogger<RegisterModel> logger;
    private readonly IEmailSender emailSender;

    public RegisterModel(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        ILogger<RegisterModel> logger,
        IEmailSender emailSender)
    {
      this.userManager = userManager;
      this.signInManager = signInManager;
      this.logger = logger;
      this.emailSender = emailSender;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public string ReturnUrl { get; set; }

    public IList<AuthenticationScheme> ExternalLogins { get; set; }

    public class InputModel
    {
      [Required(ErrorMessage = "The Email field is required")]
      [EmailAddress(ErrorMessage = "The {0} field is not a valid e-mail address")]
      [Display(Name = "Email")]
      public string Email { get; set; }

      [Required(ErrorMessage = "The Password field is required")]
      [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long", MinimumLength = 6)]
      [DataType(DataType.Password)]
      [Display(Name = "Password")]
      public string Password { get; set; }

      [DataType(DataType.Password)]
      [Display(Name = "Confirm password")]
      [Compare("Password", ErrorMessage = "The password and confirmation password do not match")]
      public string ConfirmPassword { get; set; }
    }

    public async Task OnGetAsync(string returnUrl = null)
    {
      ReturnUrl = returnUrl;
      ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
      returnUrl = returnUrl ?? Url.Content("~/");
      ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
      if (ModelState.IsValid)
      {
        var user = new IdentityUser { UserName = Input.Email, Email = Input.Email };
        var result = await userManager.CreateAsync(user, Input.Password);
        if (result.Succeeded)
        {
          logger.LogInformation("User created a new account with password.");

          var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
          code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
          var callbackUrl = Url.Page(
              "/Account/ConfirmEmail",
              pageHandler: null,
              values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
              protocol: Request.Scheme);

          await emailSender.SendEmailAsync(Input.Email, "Подтвердите вашу эл. почту",
              $"Подвердите вашу почту пройдя по <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>ссылке</a>.");

          if (userManager.Options.SignIn.RequireConfirmedAccount)
          {
            return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
          }
          else
          {
            await signInManager.SignInAsync(user, isPersistent: false);
            return LocalRedirect(returnUrl);
          }
        }
        foreach (var error in result.Errors)
        {
          ModelState.AddModelError(string.Empty, error.Description);
        }
      }

      // If we got this far, something failed, redisplay form
      return Page();
    }
  }
}
