using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Data;

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

    public AccountController(
      ILogger<AccountController> logger,
      SignInManager<IdentityUser> signInManager,
      UserManager<IdentityUser> userManager)
    {
      this.logger = logger;
      this.signInManager = signInManager;
      this.userManager = userManager;
    }

    [HttpPost]
    [Route("SignIn")]
    [AllowAnonymous]
    public async Task<IActionResult> SignIn([FromForm] LoginInputModel inputModel)
    {
      var user = await this.userManager.FindByEmailAsync(inputModel.Email);
      if (user != null)
      {
        var result = await this.signInManager.PasswordSignInAsync(user, inputModel.Password, inputModel.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
          this.logger.LogInformation($"User {user.Id} logged in");
          return this.Ok(new LoginResponse(StatusCodes.Status200OK));
        }

        if (result.IsLockedOut)
        {
          this.logger.LogInformation("Forbidden");
          return this.Forbid();
        }
      }

      this.logger.LogInformation("Unauthorized");
      return this.Unauthorized();
    }
  }
}
