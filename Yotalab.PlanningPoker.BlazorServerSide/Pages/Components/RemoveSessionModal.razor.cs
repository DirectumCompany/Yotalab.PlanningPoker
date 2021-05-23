using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Yotalab.PlanningPoker.Grains.Interfaces.Models;

namespace Yotalab.PlanningPoker.BlazorServerSide.Pages.Components
{
  public partial class RemoveSessionModal : ComponentBase
  {
    [Parameter]
    public string Id { get; set; }

    [Parameter]
    public SessionInfo Session { get; set; }

    [Parameter]
    public EventCallback<Guid> OnConfirm { get; set; }

    private Task ConfirmAsync()
    {
      return this.OnConfirm.InvokeAsync(this.Session.Id);
    }
  }
}
