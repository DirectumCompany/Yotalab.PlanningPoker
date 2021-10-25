using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Data;
using Yotalab.PlanningPoker.BlazorServerSide.Services;

namespace Yotalab.PlanningPoker.BlazorServerSide.Pages.Identity
{
  [AllowAnonymous]
  public partial class Login
  {
    private LoginInputModel loginInputModel = new();
    private bool isSubmitting = false;
    private bool success = true;
    private string[] errors = { };

    private IList<AuthenticationScheme> ExternalLogins { get; set; }

    [Inject]
    private IHttpContextAccessor HttpContextAccessor { get; set; }

    [Inject]
    private UserManager<IdentityUser> UserManager { get; set; }

    [Inject]
    private SignInManager<IdentityUser> SignInManager { get; set; }

    [Inject]
    private NavigationManager Navigation { get; set; }

    [Inject]
    private ILogger<Login> Logger { get; set; }

    [Inject]
    private JSInteropFunctions JSFunctions { get; set; }

    [Inject]
    private AuthenticationStateProvider authenticationStateProvider { get; set; }

    [Parameter]
    public string ReturnUrl { get; set; }

    private EditContext editContext;

    private ElementReference submitButton;

    private ElementReference submitHandlerFrame;

    protected override async Task OnInitializedAsync()
    {
      if (!this.HttpContextAccessor.HttpContext.Response.HasStarted)
        await this.HttpContextAccessor.HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

      var externalSchemes = await this.SignInManager.GetExternalAuthenticationSchemesAsync();
      this.ExternalLogins = externalSchemes.ToList();
      this.editContext = new EditContext(this.loginInputModel);
    }

    public string GetReturnUrl()
    {
      if (Uri.TryCreate(this.Navigation.Uri, UriKind.Absolute, out var uri))
      {
        var parameters = QueryHelpers.ParseQuery(uri.Query);
        if (parameters.TryGetValue(nameof(ReturnUrl), out var returnUrlValue))
          return returnUrlValue.FirstOrDefault();
      }
      return string.Empty;
    }

    public async Task ValidSubmit()
    {
      if (this.editContext.Validate())
      {
        if (this.isSubmitting)
        {
          this.errors = new[] { "Попробуйте обновить страницу, процесс предыдущего входа завершился неудачно" };
          return;
        }

        this.isSubmitting = true;
        await this.JSFunctions.ClickElement(this.submitButton);
      }
    }

    public async Task OnSubmitHandler(ProgressEventArgs e)
    {
      if (this.isSubmitting)
      {
        var frameContent = await this.JSFunctions.FrameInnerText(this.submitHandlerFrame);
        try
        {
          var result = JsonSerializer.Deserialize<LoginResponse>(frameContent, new JsonSerializerOptions()
          {
            PropertyNameCaseInsensitive = true
          });
          if (result != null && result.IsSuccess())
          {
            var returnUrl = this.GetReturnUrl();
            this.Navigation.NavigateTo(returnUrl, true);
          }
        }
        finally
        {
          this.isSubmitting = false;
        }
      }
    }

    public async Task OnErrorSubmitHandler(ErrorEventArgs e)
    {
      if (this.isSubmitting)
      {
        var frameContent = await this.JSFunctions.FrameInnerText(this.submitHandlerFrame);
        this.isSubmitting = false;
      }
    }
  }
}
