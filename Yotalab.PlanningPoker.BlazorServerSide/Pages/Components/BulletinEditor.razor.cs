using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Yotalab.PlanningPoker.BlazorServerSide.Resources;
using Yotalab.PlanningPoker.Grains.Interfaces.Models;

namespace Yotalab.PlanningPoker.BlazorServerSide.Pages.Components
{
  partial class BulletinEditor
  {
    private HashSet<Vote> invalidVotes = new HashSet<Vote>();

    private HashSet<string> emojiCards = new HashSet<string>()
    {
      Vote.Coffee.Value,
      Vote.IDontKnown.Value,
      "🐱‍🐉",
      "🙀",
      "🍔",
      "🍕",
      "🚀"
    };

    [Parameter]
    public Bulletin Bulletin { get; set; }

    [Parameter]
    public bool IsEditMode { get; set; }

    [Inject]
    private ISnackbar Snackbar { get; set; }

    protected override void OnParametersSet()
    {
      this.invalidVotes.Clear();
    }

    protected override void OnInitialized()
    {
      this.Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopCenter;
      this.Snackbar.Configuration.MaxDisplayedSnackbars = 1;
    }

    private void HandleCheckVote(Vote vote)
    {
      if (this.Bulletin.IsEnabled(vote))
        this.Bulletin.Disable(vote);
      else
        this.Bulletin.Enable(vote);
    }
    private void HandleRemoveVoteClick(Vote vote)
    {
      this.Snackbar.Clear();
      var items = this.Bulletin.Where(i => i.Vote.Equals(vote)).ToList();
      items.ForEach(i => this.Bulletin.Remove(i));
    }

    private void HandleAddVoteClick()
    {
      this.Snackbar.Clear();
      var numbers = this.Bulletin.Where(i => i.Vote.IsNumber);
      if (numbers.Any())
        this.Bulletin.Add(new BulletinItem(new Vote(numbers.Max(i => double.Parse(i.Vote.Value)) + 10)));
      else
        this.Bulletin.Add(new BulletinItem(new Vote(0)));
    }

    private void HandleAddEmojiVoteClick(string emoji)
    {
      this.Snackbar.Clear();
      var emojiVotes = this.Bulletin.Where(i => i.Vote.Value?.Equals(emoji) == true);
      if (!emojiVotes.Any())
        this.Bulletin.Add(new BulletinItem(new Vote(emoji)));
      else
        this.Snackbar.Add(string.Format(UIResources.BulletinItemAlreadyExists, emoji), Severity.Warning);
    }

    private void HandleVoteValueChange(Vote vote, ChangeEventArgs e)
    {
      this.Snackbar.Clear();
      this.invalidVotes.Remove(vote);

      var value = e.Value as string;
      if (string.IsNullOrWhiteSpace(value))
      {
        this.invalidVotes.Add(vote);
        this.Snackbar.Add(UIResources.BulletinItemIsEmpty, Severity.Error);
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

      if (this.Bulletin.Any(i => i.Vote.Equals(newVote) && !i.Vote.Equals(vote)))
      {
        this.invalidVotes.Add(vote);
        this.Snackbar.Add(string.Format(UIResources.BulletinItemAlreadyExists, newVote.Value), Severity.Error);
        return;
      }

      this.Bulletin.Replace(vote, newVote);
    }
  }
}
