using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Data
{
  public class ResetPasswordDetails
  {
    public ResetPasswordDetails(
      int statusCode,
      string[] errors)
    {
      StatusCode = statusCode;
      Errors = errors;
    }

    [JsonPropertyName("status")]
    public int StatusCode { get; private set; }

    [JsonPropertyName("errors")]
    public string[] Errors { get; private set; }

    public bool IsSuccess()
    {
      return this.StatusCode == StatusCodes.Status200OK;
    }

    public bool IsFailed()
    {
      return !this.IsSuccess();
    }
  }
}
