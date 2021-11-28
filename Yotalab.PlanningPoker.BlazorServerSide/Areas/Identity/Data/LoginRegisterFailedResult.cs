using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Data
{
  public class LoginRegisterFailedResult : OkObjectResult
  {
    public LoginRegisterFailedResult(IdentityResult identityResult)
      : base(new RegisterDetails(
        statusCode: StatusCodes.Status400BadRequest,
        title: "BadRequest",
        errors: identityResult.Errors?.Select(e => e.Description)?.ToArray()))
    {
    }
  }
}
