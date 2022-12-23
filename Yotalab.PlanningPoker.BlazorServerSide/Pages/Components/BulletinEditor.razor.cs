using Microsoft.AspNetCore.Components;
using Yotalab.PlanningPoker.Grains.Interfaces.Models;

namespace Yotalab.PlanningPoker.BlazorServerSide.Pages.Components
{
  partial class BulletinEditor
  {
    [Parameter]
    public Bulletin Bulletin { get; set; }

    private void HandleCheckVote(Vote vote)
    {
      if (this.Bulletin.IsEnabled(vote))
        this.Bulletin.Disable(vote);
      else
        this.Bulletin.Enable(vote);
    }
  }
}
