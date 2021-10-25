using System;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Data
{
  public class LoginResponse
  {
    public LoginResponse(int statusCode)
    {
      this.StatusCode = statusCode;
    }

    [JsonInclude]
    public int StatusCode { get; private set; }

    public bool IsSuccess()
    {
      return StatusCode == StatusCodes.Status200OK;
    }

    private LoginResponse() { }
  }
}
