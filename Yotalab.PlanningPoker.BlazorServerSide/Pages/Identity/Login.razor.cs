using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using MudBlazor;

namespace Yotalab.PlanningPoker.BlazorServerSide.Pages.Identity
{
  [AllowAnonymous]
  public partial class Login
  {
    private EditForm form;
    private LoginInputModel loginInputModel = new LoginInputModel();
    private bool success = true;
    private string[] errors = { };

    [Inject]
    private IHttpClientFactory HttpClientFactory { get; set; }

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

    [Parameter]
    public string ReturnUrl { get; set; }

    protected override async Task OnInitializedAsync()
    {
      if (!this.HttpContextAccessor.HttpContext.Response.HasStarted)
        await this.HttpContextAccessor.HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

      var externalSchemes = await this.SignInManager.GetExternalAuthenticationSchemesAsync();
      this.ExternalLogins = externalSchemes.ToList();
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

    public async Task SignIn(EditContext editContext)
    {
      var user = await this.UserManager.FindByEmailAsync(this.loginInputModel.Email);
      var result = await this.SignInManager.CheckPasswordSignInAsync(user, this.loginInputModel.Password, this.loginInputModel.RememberMe);
      if (result.Succeeded)
      {
        this.Logger.LogInformation("User {UserEmail} logged in.", this.loginInputModel.Email);
        var httpClient = this.HttpClientFactory.CreateClient();
        var data = new FormUrlEncodedContent(new[]
        {
          new KeyValuePair<string,string>($"InputModel.{nameof(this.loginInputModel.Email)}", this.loginInputModel.Email),
          new KeyValuePair<string,string>($"InputModel.{nameof(this.loginInputModel.Password)}", this.loginInputModel.Email),
          new KeyValuePair<string,string>($"InputModel.{nameof(this.loginInputModel.RememberMe)}", this.loginInputModel.RememberMe.ToString()),
          new KeyValuePair<string,string>($"InputModel.{nameof(this.ReturnUrl)}", this.GetReturnUrl())
        });
        // см. https://stackoverflow.com/questions/59121741/anti-forgery-token-validation-in-mvc-app-with-blazor-server-side-component
        // ?email={this.loginInputModel.Email}&password={this.loginInputModel.Password}&rememberMe={this.loginInputModel.RememberMe}&returnUrl={this.GetReturnUrl()}
        var uri = $"{this.Navigation.BaseUri}/Identity/api/Values/Login?email={this.loginInputModel.Email}&password={this.loginInputModel.Password}&rememberMe={this.loginInputModel.RememberMe}&returnUrl={this.GetReturnUrl()}";
        var resultPost = await httpClient.PostAsync(uri, data);
      }
      if (result.IsLockedOut)
      {
        this.Logger.LogWarning("User account locked out.");
      }
    }
  }

  public class LoginInputModel
  {
    [Required(ErrorMessage = "The Email field is required")]
    [EmailAddress(ErrorMessage = "The {0} field is not a valid e-mail address")]
    [Display(Name = "Email")]
    public string Email { get; set; }

    [Required(ErrorMessage = "The Password field is required")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; }

    [Display(Name = "Remember Me")]
    public bool RememberMe { get; set; }
  }
}
