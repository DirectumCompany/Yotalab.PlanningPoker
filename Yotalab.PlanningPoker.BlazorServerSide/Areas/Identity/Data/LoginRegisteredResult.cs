using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Data
{
  public class LoginRegisteredResult : OkObjectResult
  {
    public LoginRegisteredResult(bool requireConfirmation)
      : base(new RegisterDetails(
        statusCode: StatusCodes.Status200OK,
        confirmRequired: requireConfirmation))
    {
    }
  }
}
