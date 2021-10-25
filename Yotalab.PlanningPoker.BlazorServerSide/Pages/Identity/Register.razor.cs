using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Data;
using Yotalab.PlanningPoker.BlazorServerSide.Services.Mailing;

namespace Yotalab.PlanningPoker.BlazorServerSide.Pages.Identity
{
  public partial class Register
  {
    private EditForm form;
    private RegisterInputModel inputModel = new();
    private bool success = true;
    private string[] errors = { };

    [Inject]
    private SignInManager<IdentityUser> signInManager { get; set; }

    [Inject]
    private UserManager<IdentityUser> userManager { get; set; }

    [Inject]
    private ILogger<Register> logger { get; set; }

    [Inject]
    private IEmailSender emailSender { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; }

    [Parameter]
    public string ReturnUrl { get; set; }

    public async Task SignUp(EditContext editContext)
    {
      if (this.success)
      {
        var user = new IdentityUser { UserName = this.inputModel.Email, Email = this.inputModel.Email };
        var result = await userManager.CreateAsync(user, this.inputModel.Password);
        if (result.Succeeded)
        {
          logger.LogInformation("User created a new account with password.");

          var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
          code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
          var callbackUrl = this.NavigationManager.ToAbsoluteUri($"identity/confirm?userId={user.Id}&code={code}&returnUrl={this.ReturnUrl}");
          await emailSender.SendEmailAsync(this.inputModel.Email, "Подтвердите вашу эл. почту",
              $"Подвердите вашу почту пройдя по <a href='{HtmlEncoder.Default.Encode(callbackUrl.ToString())}'>ссылке</a>.");

          if (this.userManager.Options.SignIn.RequireConfirmedAccount)
          {
            /*
              <h1 class="h3 mb-3 fw-normal">Подтверждение регистрации</h1>
              <p>
                На вашу почту отправлено письмо для подтверждения регистрации.
              </p>
            */
            this.errors = new[] { "Ok" };
            return;
          }
          else
          {
            await signInManager.SignInAsync(user, isPersistent: false);
            this.errors = new[] { "Ok" };
            return;
          }
        }
        this.errors = result.Errors.Select(e => e.Description).ToArray();
      }

      await Task.CompletedTask;
    }
  }
}
