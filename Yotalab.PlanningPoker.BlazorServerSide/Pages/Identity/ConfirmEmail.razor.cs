using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;

namespace Yotalab.PlanningPoker.BlazorServerSide.Pages.Identity
{
  public partial class ConfirmEmail
  {
    private bool isSuccess;

    private bool notFound;

    [Inject]
    private NavigationManager Navigation { get; set; }

    protected override void OnInitialized()
    {
      var uri = this.Navigation.ToAbsoluteUri(this.Navigation.Uri);
      if (!string.IsNullOrWhiteSpace(uri.Query))
      {
        var queryMap = QueryHelpers.ParseQuery(uri.Query);

        var result = string.Empty;
        if (queryMap.TryGetValue("result", out var userIdValue))
          result = userIdValue.ToString();

        this.isSuccess = result == "success";
        this.notFound = result == "notFound";
      }
    }
  }
}
