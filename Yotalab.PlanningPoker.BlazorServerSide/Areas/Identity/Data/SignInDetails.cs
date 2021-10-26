using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Data
{
  public class SignInDetails
  {
    [JsonConstructor]
    public SignInDetails(
      int statusCode,
      string title = null,
      bool isLockedOut = false,
      bool isNotAllowed = false)
    {
      this.StatusCode = statusCode;
      this.Title = title;
      this.IsLockedOut = isLockedOut;
      this.IsNotAllowed = isNotAllowed;
    }

    [JsonPropertyName("title")]
    public string Title { get; private set; }

    [JsonPropertyName("status")]
    public int StatusCode { get; private set; }

    [JsonPropertyName("locked")]
    public bool IsLockedOut { get; private set; }

    [JsonPropertyName("notAllowed")]
    public bool IsNotAllowed { get; private set; }

    public bool IsSuccess()
    {
      return this.StatusCode == StatusCodes.Status200OK;
    }

    public bool IsForbidden()
    {
      return this.StatusCode == StatusCodes.Status403Forbidden;
    }

    public bool IsUnauthorized()
    {
      return this.StatusCode == StatusCodes.Status401Unauthorized;
    }
  }
}
