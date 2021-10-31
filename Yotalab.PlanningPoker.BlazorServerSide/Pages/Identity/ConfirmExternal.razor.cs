using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;

namespace Yotalab.PlanningPoker.BlazorServerSide.Pages.Identity
{
  public partial class ConfirmExternal
  {
    private bool isSuccess;
    private List<string> errors = new();

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
        if (!this.isSuccess)
        {
          if (queryMap.TryGetValue("error", out var errorValues))
          {
            foreach (var error in errorValues)
            {
              this.errors.Add(error);
            }
          }
          return;
        }
      }

      this.Navigation.NavigateTo("/", true);
    }
  }
}
