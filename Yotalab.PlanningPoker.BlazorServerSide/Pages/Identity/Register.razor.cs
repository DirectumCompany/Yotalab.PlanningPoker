using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Data;
using Yotalab.PlanningPoker.BlazorServerSide.Services;

namespace Yotalab.PlanningPoker.BlazorServerSide.Pages.Identity
{
  public partial class Register
  {
    private RegisterInputModel inputModel = new();
    private List<string> errors = new();
    private EditContext editContext;
    private bool isSubmitting = false;
    private bool showRequiredConfirmation = false;
    private ElementReference submitButton;
    private ElementReference submitHandlerFrame;

    [Inject]
    private NavigationManager Navigation { get; set; }

    [Inject]
    private JSInteropFunctions JSFunctions { get; set; }

    [Inject]
    private ILogger<Login> Logger { get; set; }

    [Parameter]
    public string ReturnUrl { get; set; }

    protected override Task OnInitializedAsync()
    {
      this.editContext = new EditContext(this.inputModel);
      return Task.CompletedTask;
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
          this.errors.Add("Попробуйте обновить страницу, предыдущий процесс регистрации завершился неудачно");
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
          var result = JsonSerializer.Deserialize<RegisterDetails>(frameContent, options);
          if (result != null)
          {
            if (result.IsSuccess())
            {
              if (result.ConfirmRequired)
              {
                this.showRequiredConfirmation = true;
              }
              else
              {
                var returnUrl = this.GetReturnUrl();
                this.Navigation.NavigateTo(returnUrl, true);
              }
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
          this.Logger.LogWarning(ex, "Sign up failed.");
          this.errors.Clear();
          this.errors.Add("Неудачная регистрация, повторите позже");
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
        this.errors.Add("Неудачная регистрация, повторите позже");
        this.isSubmitting = false;
        this.StateHasChanged();
      }
    }
  }
}
