using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Data;

namespace Yotalab.PlanningPoker.BlazorServerSide.Pages.Identity
{
  public partial class Confirm
  {
    private IdentityResult result;

    [Inject]
    private NavigationManager Navigation { get; set; }

    protected override void OnInitialized()
    {
      var uri = this.Navigation.ToAbsoluteUri(this.Navigation.Uri);
      if (!string.IsNullOrWhiteSpace(uri.Query))
      {
        var queryMap = QueryHelpers.ParseQuery(uri.Query);
        if (queryMap.TryGetValue("result", out var encodedIdentityResult))
        {
          this.result = IdentityResultEncoder.Base64UrlDecode(encodedIdentityResult.ToString());
        }
      }
    }
  }
}
