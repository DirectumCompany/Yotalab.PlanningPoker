using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Data
{
  public class RegisterDetails
  {
    [JsonConstructor]
    public RegisterDetails(
      int statusCode,
      string title = null,
      string[] errors = null,
      bool confirmRequired = false)
    {
      this.StatusCode = statusCode;
      this.Title = title;
      this.Errors = errors?.ToArray();
      this.ConfirmRequired = confirmRequired;
    }

    [JsonPropertyName("title")]
    public string Title { get; private set; }

    [JsonPropertyName("status")]
    public int StatusCode { get; private set; }

    [JsonPropertyName("confirmRequired")]
    public bool ConfirmRequired { get; private set; }

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
