using MudBlazor;

namespace Yotalab.PlanningPoker.BlazorServerSide.Shared
{
  public static class Themes
  {
    public static string GetName(MudTheme theme)
    {
      return theme == NightMode ? nameof(NightMode) : nameof(Default);
    }

    public static MudTheme GetTheme(string themeName)
    {
      return themeName == nameof(NightMode) ? NightMode : Default;
    }

    public static readonly MudTheme Default = new()
    {
      Palette = new Palette()
      {
        Background = "#f8f9fa",
      }
    };

    public static readonly MudTheme NightMode = new()
    {
      Palette = new Palette()
      {
        Black = "#27272f",
        Background = "#32333d",
        BackgroundGrey = "#27272f",
        Surface = "#373740",
        DrawerBackground = "#27272f",
        DrawerText = "rgba(255,255,255, 0.50)",
        DrawerIcon = "rgba(255,255,255, 0.50)",
        AppbarBackground = "#27272f",
        AppbarText = "rgba(255,255,255, 0.70)",
        TextPrimary = "rgba(255,255,255, 0.70)",
        TextSecondary = "rgba(255,255,255, 0.50)",
        ActionDefault = "#adadb1",
        ActionDisabled = "rgba(255,255,255, 0.26)",
        ActionDisabledBackground = "rgba(255,255,255, 0.12)",
        Divider = "rgba(255,255,255, 0.12)",
        DividerLight = "rgba(255,255,255, 0.06)",
        TableLines = "rgba(255,255,255, 0.12)",
        LinesDefault = "rgba(255,255,255, 0.12)",
        LinesInputs = "rgba(255,255,255, 0.3)",
        TextDisabled = "rgba(255,255,255, 0.2)"
      }
    };
  }
}
