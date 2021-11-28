using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Data
{
  public class LoginSignedInResult : OkObjectResult
  {
    public LoginSignedInResult()
      : base(new SignInDetails(StatusCodes.Status200OK))
    {
    }
  }
}
