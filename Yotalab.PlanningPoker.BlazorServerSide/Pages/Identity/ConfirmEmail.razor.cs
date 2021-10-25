using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace Yotalab.PlanningPoker.BlazorServerSide.Pages.Identity
{
  public partial class ConfirmEmail
  {
    private bool isConfirmed;

    [Inject]
    private UserManager<IdentityUser> userManager { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; }

    protected override async Task OnInitializedAsync()
    {
      if (this.isConfirmed)
        return;

      var uri = this.NavManager.ToAbsoluteUri(this.NavManager.Uri);
      if (!string.IsNullOrWhiteSpace(uri.Query))
      {
        var queryMap = QueryHelpers.ParseQuery(uri.Query);

        var userId = string.Empty;
        if (queryMap.TryGetValue("userId", out var userIdValue))
          userId = userIdValue.ToString();

        string code = string.Empty;
        if (queryMap.TryGetValue("code", out var codeValue))
          code = codeValue.ToString();

        string returnUrl = string.Empty;
        if (queryMap.TryGetValue("returnUrl", out var returnUrlValue))
          returnUrl = returnUrlValue.ToString();

        var user = await userManager.FindByIdAsync(userId);
        if (user != null)
        {
          code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
          var result = await userManager.ConfirmEmailAsync(user, code);
          this.isConfirmed = true;
        }
      }
    }
  }
}
