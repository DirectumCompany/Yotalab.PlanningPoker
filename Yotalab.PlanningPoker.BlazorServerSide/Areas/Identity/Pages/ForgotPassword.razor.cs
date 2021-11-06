using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Data;

namespace Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Pages
{
  public partial class ForgotPassword
  {
    private ForgotPasswordInputModel inputModel = new();
    private bool showSuccessful;

    [Inject]
    private IHttpClientFactory HttpClientFactory { get; set; }

    [Inject]
    private NavigationManager Navigation { get; set; }

    private async Task ValidSubmit()
    {
      this.showSuccessful = false;
      var httpClient = this.HttpClientFactory.CreateClient();
      var response = await httpClient.PostAsync($"{this.Navigation.BaseUri}/api/identity/account/forgotPassword", new FormUrlEncodedContent(new Dictionary<string, string>()
      {
        { nameof(ForgotPasswordInputModel.Email), this.inputModel.Email }
      }));
      if (response.IsSuccessStatusCode)
        this.showSuccessful = true;
    }
  }
}
