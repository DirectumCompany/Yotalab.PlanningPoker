using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Yotalab.PlanningPoker.BlazorServerSide.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class CultureController : ControllerBase
  {
    [HttpGet]
    [Route("Set")]
    public IActionResult Set(string culture, string redirectUri)
    {
      if (culture != null)
      {
        this.HttpContext.Response.Cookies.Append(
          CookieRequestCultureProvider.DefaultCookieName,
          CookieRequestCultureProvider.MakeCookieValue(
            new RequestCulture(culture, culture)));
      }

      return LocalRedirect(redirectUri);
    }
  }
}
