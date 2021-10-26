﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
    private List<string> errors = new();

    private IList<AuthenticationScheme> ExternalLogins { get; set; }

    [Inject]
    private IHttpContextAccessor HttpContextAccessor { get; set; }

    [Inject]
    private SignInManager<IdentityUser> SignInManager { get; set; }

    [Inject]
    private NavigationManager Navigation { get; set; }

    [Inject]
    private ILogger<Login> Logger { get; set; }

    [Inject]
    private JSInteropFunctions JSFunctions { get; set; }

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
        this.errors.Clear();
        if (this.isSubmitting)
        {
          this.errors.Add("Попробуйте обновить страницу, процесс предыдущего входа завершился неудачно");
          return;
        }

        this.isSubmitting = true;
        await this.JSFunctions.ClickElement(this.submitButton);
      }
    }

    public void InvalidSubmit()
    {
      this.errors.Clear();
    }

    public async Task OnSubmitHandler(ProgressEventArgs e)
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
                this.errors.Add("Учетная запись заблокировна, повторите попытку позже.");
              else if (result.IsNotAllowed)
                this.errors.Add("Учетная запись не подтверждена.");
            }
            else if (result.IsUnauthorized())
            {
              this.errors.Clear();
              this.errors.Add("Неудачная попытка входа.");
            }
          }
        }
        catch (Exception ex)
        {
          this.Logger.LogWarning(ex, "Sign in failed.");
          this.errors.Clear();
          this.errors.Add("Неудачная попытка входа.");
        }
        finally
        {
          this.isSubmitting = false;
        }
      }
    }

    public void OnErrorSubmitHandler(ErrorEventArgs e)
    {
      if (this.isSubmitting)
      {
        this.errors.Clear();
        this.errors.Add("Неудачная попытка входа.");
        this.isSubmitting = false;
        this.StateHasChanged();
      }
    }
  }
}
