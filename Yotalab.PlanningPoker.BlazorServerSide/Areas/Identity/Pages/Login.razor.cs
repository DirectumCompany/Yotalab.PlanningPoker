using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Data;
using Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Resources;
using Yotalab.PlanningPoker.BlazorServerSide.Services;

namespace Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Pages
{
  [AllowAnonymous]
  public partial class Login
  {
    private LoginInputModel loginInputModel = new();
    private bool isSubmitting = false;
    private bool success = true;
    private List<string> errors = new();
    private EditContext editContext;
    private ElementReference submitButton;
    private ElementReference submitHandlerFrame;

    private IList<AuthenticationScheme> ExternalLogins { get; set; }

    [Inject]
    private SignInManager<IdentityUser> SignInManager { get; set; }

    [Inject]
    private NavigationManager Navigation { get; set; }

    [Inject]
    private ILogger<Login> Logger { get; set; }

    [Inject]
    private JSInteropFunctions JSFunctions { get; set; }

    protected override async Task OnInitializedAsync()
    {
      var externalSchemes = await this.SignInManager.GetExternalAuthenticationSchemesAsync();
      this.ExternalLogins = externalSchemes.ToList();
      this.editContext = new EditContext(this.loginInputModel);
    }

    private string GetReturnUrl()
    {
      if (Uri.TryCreate(this.Navigation.Uri, UriKind.Absolute, out var uri))
      {
        var parameters = QueryHelpers.ParseQuery(uri.Query);
        if (parameters.TryGetValue("returnUrl", out var returnUrlValue))
          return returnUrlValue.FirstOrDefault();
      }
      return string.Empty;
    }

    private async Task ValidSubmit()
    {
      if (this.editContext.Validate())
      {
        this.errors.Clear();
        if (this.isSubmitting)
        {
          this.errors.Add(IdentityUIResources.SignInAttemptFailed);
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
          var result = JsonSerializer.Deserialize<SignInDetails>(frameContent, options);
          if (result != null)
          {
            if (result.IsSuccess())
            {
              var returnUrl = this.GetReturnUrl();
              this.Navigation.NavigateTo(returnUrl, true);
            }
            else if (result.IsForbidden())
            {
              this.errors.Clear();
              if (result.IsLockedOut)
                this.errors.Add(IdentityResources.UserLockedOut);
              else if (result.IsNotAllowed)
                this.errors.Add(IdentityUIResources.UserNotConfirmEmail);
            }
            else if (result.IsUnauthorized())
            {
              this.errors.Clear();
              this.errors.Add(IdentityUIResources.SignInAttemptFailed);
            }
          }
        }
        catch (Exception ex)
        {
          this.Logger.LogWarning(ex, "Sign in failed.");
          this.errors.Clear();
          this.errors.Add(IdentityUIResources.SignInAttemptFailed);
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
        this.errors.Add(IdentityUIResources.SignInAttemptFailed);
        this.isSubmitting = false;
        this.StateHasChanged();
      }
    }
  }
}
