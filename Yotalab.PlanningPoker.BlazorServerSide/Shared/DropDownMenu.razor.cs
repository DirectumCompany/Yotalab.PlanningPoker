using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Yotalab.PlanningPoker.BlazorServerSide.Shared
{
  public class DropDownMenuItem
  {
    public string ItemContainerClassName { get; set; }

    public Action<MouseEventArgs> OnClick { get; set; }

    public Func<MouseEventArgs, Task> OnClickAsync { get; set; }

    public RenderFragment ItemTemplate { get; set; }
  }
}
