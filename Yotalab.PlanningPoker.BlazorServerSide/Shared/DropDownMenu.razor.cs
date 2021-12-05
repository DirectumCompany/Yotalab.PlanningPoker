using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazorColor = MudBlazor.Color;

namespace Yotalab.PlanningPoker.BlazorServerSide.Shared
{
  public partial class DropDownMenu
  {
    [Parameter]
    public string Icon { get; set; }

    [Parameter]
    public RenderFragment ButtonTemplate { get; set; }

    [Parameter]
    public RenderFragment ChildContent { get; set; }

    [Parameter]
    public IEnumerable<DropDownMenuItem> Items { get; set; }

    private EventCallback<MouseEventArgs> CreateOnClickCallback(DropDownMenuItem item)
    {
      var factory = new EventCallbackFactory();
      return item.OnClick != null ?
        factory.Create(this, item.OnClick) :
        factory.Create(this, item.OnClickAsync);
    }
  }

  public class DropDownMenuItem
  {
    public string Title { get; set; }

    public string Icon { get; set; }

    public MudBlazorColor IconColor { get; set; } = MudBlazorColor.Default;

    public Action<MouseEventArgs> OnClick { get; set; }

    public Func<MouseEventArgs, Task> OnClickAsync { get; set; }
  }
}
