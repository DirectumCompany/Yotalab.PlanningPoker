using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Yotalab.PlanningPoker.BlazorServerSide.Pages.Identity
{
  public partial class ForgotPassword
  {
    private string email = string.Empty;
    private bool showSuccessful;

    [Inject]
    private IHttpClientFactory HttpClientFactory { get; set; }

    [Inject]
    private NavigationManager Navigation { get; set; }

    private async Task ValidSubmit()
    {
      this.showSuccessful = false;
      var httpClient = this.HttpClientFactory.CreateClient();
      var response = await httpClient.PostAsync($"{this.Navigation.BaseUri}/identity/account/forgotPassword", new FormUrlEncodedContent(new Dictionary<string, string>()
      {
        { "email", this.email }
      }));
      if (response.IsSuccessStatusCode)
        this.showSuccessful = true;
    }
  }
}
