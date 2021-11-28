using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using IdentitySignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Data
{
  public class LoginForbiddenResult : ForbidResult
  {
    private readonly IdentitySignInResult signInResult;

    public LoginForbiddenResult(IdentitySignInResult signInResult)
    {
      this.signInResult = signInResult;
    }

    public override async Task ExecuteResultAsync(ActionContext context)
    {
      context.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
      var objectResult = new ObjectResult(new SignInDetails(StatusCodes.Status403Forbidden, "Forbidden", this.signInResult.IsLockedOut, this.signInResult.IsNotAllowed));
      var executor = context.HttpContext.RequestServices.GetRequiredService<IActionResultExecutor<ObjectResult>>();
      await executor.ExecuteAsync(context, objectResult);

      await base.ExecuteResultAsync(context);
    }
  }
}
