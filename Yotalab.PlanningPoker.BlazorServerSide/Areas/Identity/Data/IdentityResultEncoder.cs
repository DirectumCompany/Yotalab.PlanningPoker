using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Yotalab.PlanningPoker.Hosting;

namespace Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Data
{
  public static class IdentityResultEncoder
  {
    public static string Base64UrlEncode(IdentityResult identityResult)
    {
      var result = JsonConvert.SerializeObject(identityResult);
      return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(result));
    }

    public static IdentityResult Base64UrlDecode(string encodedIdentityResult)
    {
      var decodedResult = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(encodedIdentityResult));
      var settings = new JsonSerializerSettings();
      settings.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
      settings.ContractResolver = new PrivateSetterContractResolver();
      return JsonConvert.DeserializeObject<IdentityResult>(decodedResult, settings);
    }
  }
}
