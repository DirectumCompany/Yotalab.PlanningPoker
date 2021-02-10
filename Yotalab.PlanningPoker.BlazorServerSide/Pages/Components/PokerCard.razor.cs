using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Yotalab.PlanningPoker.BlazorServerSide.Services;
using Yotalab.PlanningPoker.Grains.Interfaces.Models;

namespace Yotalab.PlanningPoker.BlazorServerSide.Pages.Components
{
  public partial class PokerCard : OwningComponentBase<SessionService>
  {
    private bool isSelected;

    [Parameter]
    public Guid SessionId { get; set; }

    [Parameter]
    public Vote Value { get; set; }

    [Parameter]
    public bool Selected { get; set; }

    [Parameter]
    public EventCallback<Vote> OnSelectedChanged { get; set; }

    protected override void OnParametersSet()
    {
      this.isSelected = this.Selected;
      base.OnParametersSet();
    }

    private Task OnPokerCardClick()
    {
      this.isSelected = !this.isSelected;
      return this.OnSelectedChanged.InvokeAsync(this.isSelected ? this.Value : Vote.Unset);
    }
  }
}
