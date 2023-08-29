using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Data;
using Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Resources;
using Yotalab.PlanningPoker.BlazorServerSide.Services;

namespace Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Pages
{
  public partial class ResetPassword
  {
    private List<string> errors = new();
    private ElementReference submitHandlerFrame;
    private bool isSubmitting = false;
    private bool showResetSuccessful;
    private ElementReference submitButton;
    private ResetPasswordInputModel inputModel = new();
    private EditContext editContext;

    [Inject]
    private NavigationManager Navigation { get; set; }

    [Inject]
    private JSInteropFunctions JSFunctions { get; set; }

    [Inject]
    private ILogger<Login> Logger { get; set; }

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
        this.errors.Clear();
        if (this.isSubmitting)
        {
          this.errors.Add(IdentityUIResources.ResetPasswordFailed);
          return;
        }

        this.isSubmitting = true;
        await this.JSFunctions.ClickElement(this.submitButton);
      }
    }

    private void InvalidSubmit()
    {
      this.errors.Clear();
    }

    private async Task OnSubmitHandler(ProgressEventArgs e)
    {
      if (this.isSubmitting)
      {
        var frameContent = await this.JSFunctions.FrameInnerText(this.submitHandlerFrame);
        try
        {
          var options = new JsonSerializerOptions()
          {
            PropertyNameCaseInsensitive = true
          };
          var result = JsonSerializer.Deserialize<ResetPasswordDetails>(frameContent, options);
          if (result != null)
          {
            if (result.IsSuccess())
            {
              this.showResetSuccessful = true;
            }
            else if (result.IsFailed())
            {
              this.errors.Clear();
              this.errors.AddRange(result.Errors);
            }
          }
        }
        catch (Exception ex)
        {
          this.Logger.LogWarning(ex, "Reset password failed.");
          this.errors.Clear();
          this.errors.Add(IdentityUIResources.ResetPasswordFailed);
        }
        finally
        {
          this.isSubmitting = false;
        }
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
