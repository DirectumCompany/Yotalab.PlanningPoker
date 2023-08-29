using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Data
{
  public class ResetPasswordResult : OkObjectResult
  {
    public ResetPasswordResult()
      : base(new ResetPasswordDetails(statusCode: StatusCodes.Status200OK, Array.Empty<string>()))
    {
    }
  }
}
