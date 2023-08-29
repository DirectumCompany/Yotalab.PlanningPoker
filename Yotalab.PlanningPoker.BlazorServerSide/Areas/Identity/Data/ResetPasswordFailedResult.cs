using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Data
{
  public class ResetPasswordFailedResult : OkObjectResult
  {
    public ResetPasswordFailedResult(IdentityResult identityResult)
      : base(new ResetPasswordDetails(
        statusCode: StatusCodes.Status400BadRequest,
        errors: identityResult.Errors?.Select(e => e.Description)?.ToArray()))
    {
    }
  }
}
