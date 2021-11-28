using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;
using MudBlazorColor = MudBlazor.Color;

namespace Yotalab.PlanningPoker.BlazorServerSide.Shared
{
  public class DropDownMenuItem
  {
    public string Title { get; set; }

    public string Icon { get; set; }

    public MudBlazorColor IconColor { get; set; } = MudBlazorColor.Default;

    public Action<MouseEventArgs> OnClick { get; set; }

    public Func<MouseEventArgs, Task> OnClickAsync { get; set; }
  }
}
