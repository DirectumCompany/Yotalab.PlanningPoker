using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.WebUtilities;
using Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Data;
using Yotalab.PlanningPoker.BlazorServerSide.Services;

namespace Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Pages
{
  public partial class ResetPassword
  {
    private bool isSubmitting = false;
    private bool showResetSuccessful;
    private ElementReference submitButton;
    private ResetPasswordInputModel inputModel = new();
    private EditContext editContext;

    [Inject]
    private NavigationManager Navigation { get; set; }

    [Inject]
    private JSInteropFunctions JSFunctions { get; set; }

    protected override void OnInitialized()
    {
      var uri = this.Navigation.ToAbsoluteUri(this.Navigation.Uri);
      if (!string.IsNullOrWhiteSpace(uri.Query))
      {
        var queryMap = QueryHelpers.ParseQuery(uri.Query);

        if (queryMap.TryGetValue("code", out var code))
          this.inputModel.Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code.ToString()));
      }

      this.editContext = new EditContext(this.inputModel);
    }

    private async Task ValidSubmit()
    {
      if (this.editContext.Validate())
      {
        if (this.isSubmitting)
        {
          return;
        }

        this.isSubmitting = true;
        await this.JSFunctions.ClickElement(this.submitButton);
      }
    }

    private void OnSubmitHandler(ProgressEventArgs e)
    {
      if (this.isSubmitting)
      {
        this.isSubmitting = false;
        this.showResetSuccessful = true;
      }
    }

    private void OnErrorSubmitHandler(ErrorEventArgs e)
    {
      if (this.isSubmitting)
      {
        this.isSubmitting = false;
        this.StateHasChanged();
      }
    }
  }
}
