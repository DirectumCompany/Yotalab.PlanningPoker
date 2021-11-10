using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Data;
using Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Resources;
using Yotalab.PlanningPoker.BlazorServerSide.Services;

namespace Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Pages
{
  public partial class ForgotPassword
  {
    private List<string> errors = new();
    private ElementReference submitButton;
    private ForgotPasswordInputModel inputModel = new();
    private EditContext editContext;
    private bool showSuccessful;
    private bool isSubmitting = false;

    [Inject]
    private JSInteropFunctions JSFunctions { get; set; }

    protected override void OnInitialized()
    {
      this.editContext = new EditContext(this.inputModel);
    }

    private async Task ValidSubmit()
    {
      this.errors.Clear();
      if (this.isSubmitting)
      {
        this.errors.Add(IdentityUIResources.ResetPasswordFailed);
        return;
      }

      this.isSubmitting = true;
      await this.JSFunctions.ClickElement(this.submitButton);
    }

    private void OnSubmitHandler(ProgressEventArgs e)
    {
      if (this.isSubmitting)
      {
        this.showSuccessful = true;
      }
    }

    private void OnErrorSubmitHandler(ErrorEventArgs e)
    {
      if (this.isSubmitting)
      {
        this.errors.Clear();
        this.errors.Add(IdentityUIResources.ResetPasswordFailed);
        this.isSubmitting = false;
        this.StateHasChanged();
      }
    }
  }
}
