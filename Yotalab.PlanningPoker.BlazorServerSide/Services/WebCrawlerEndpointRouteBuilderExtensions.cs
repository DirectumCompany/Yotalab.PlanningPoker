using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Yotalab.PlanningPoker.BlazorServerSide.Services
{
  internal static class WebCrawlerEndpointRouteBuilderExtensions
  {
    private const string SitemapDefaultTemplate = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9""
        xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
        xsi:schemaLocation=""http://www.sitemaps.org/schemas/sitemap/0.9
        http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd"">
  <url>
    <loc>{0}</loc>
  </url>
</urlset>
";

    private const string RobotsDefaultTemplate = @"User-Agent: *
Disallow: /identity/
Allow: /

Sitemap: {0}sitemap.xml
";

    public static IEndpointConventionBuilder MapRobots(this IEndpointRouteBuilder endpoints)
    {
      return endpoints.MapGet("robots.txt", (HttpContext context) =>
      {
        return string.Format(RobotsDefaultTemplate, GetHostUrl(context));
      });
    }

    public static IEndpointConventionBuilder MapSitemap(this IEndpointRouteBuilder endpoints)
    {
      return endpoints.MapGet("sitemap.xml", (HttpContext context) =>
      {
        return string.Format(SitemapDefaultTemplate, GetHostUrl(context));
      });
    }

    private static string GetHostUrl(HttpContext context) => $"{context.Request.Scheme}://{context.Request.Host}/";
  }
}
