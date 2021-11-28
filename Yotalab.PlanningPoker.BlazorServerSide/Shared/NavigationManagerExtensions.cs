using System;
using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace Yotalab.PlanningPoker.BlazorServerSide.Shared
{
  public static class NavigationManagerExtensions
  {
    public static void NavigateToSetCulture(this NavigationManager navigation, CultureInfo culture)
    {
      if (CultureInfo.CurrentCulture != culture)
      {
        var uri = new Uri(navigation.Uri).GetComponents(UriComponents.PathAndQuery, UriFormat.Unescaped);
        var cultureEscaped = Uri.EscapeDataString(culture.Name);
        var uriEscaped = Uri.EscapeDataString(uri);

        navigation.NavigateTo($"api/culture/set?culture={cultureEscaped}&redirectUri={uriEscaped}", forceLoad: true);
      }
    }
  }
}
