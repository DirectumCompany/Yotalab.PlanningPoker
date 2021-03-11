using Microsoft.AspNetCore.Components;

namespace Yotalab.PlanningPoker.BlazorServerSide.Shared
{
  public class DropDownMenuItem
  {
    public string ItemContainerClassName { get; set; }

    public RenderFragment ItemTemplate { get; set; }
  }
}
