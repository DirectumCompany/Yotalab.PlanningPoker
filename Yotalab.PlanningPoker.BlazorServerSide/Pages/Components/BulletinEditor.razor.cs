using System.Linq;
using Microsoft.AspNetCore.Components;
using Yotalab.PlanningPoker.Grains.Interfaces.Models;

namespace Yotalab.PlanningPoker.BlazorServerSide.Pages.Components
{
  partial class BulletinEditor
  {
    [Parameter]
    public Bulletin Bulletin { get; set; }

    [Parameter]
    public bool IsEditMode { get; set; }

    private void HandleCheckVote(Vote vote)
    {
      if (this.Bulletin.IsEnabled(vote))
        this.Bulletin.Disable(vote);
      else
        this.Bulletin.Enable(vote);
    }
    private void HandleRemoveVoteClick(Vote vote)
    {
      var item = this.Bulletin.Single(i => i.Vote.Equals(vote));
      this.Bulletin.Remove(item);
    }

    private void HandleAddVoteClick()
    {
      var numbers = this.Bulletin.Where(i => i.Vote.IsNumber);
      if (numbers.Any())
        this.Bulletin.Add(new BulletinItem(new Vote(numbers.Max(i => double.Parse(i.Vote.Value)) + 10)));
      else
        this.Bulletin.Add(new BulletinItem(new Vote(0)));
    }

    private void HandleVoteValueChange(Vote vote, ChangeEventArgs e)
    {
      var value = e.Value as string;
      if (string.IsNullOrWhiteSpace(value))
      {
        return;
      }

      Vote newVote;
      if (int.TryParse(value, out var intValue))
      {
        newVote = new Vote(intValue);
      }
      else if (double.TryParse(value, out var doubleValue))
      {
        newVote = new Vote(doubleValue);
      }
      else
      {
        newVote = new Vote(value);
      }

      this.Bulletin.Replace(vote, newVote);
    }
  }
}
