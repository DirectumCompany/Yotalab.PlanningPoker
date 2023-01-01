using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Utilities;
using Yotalab.PlanningPoker.BlazorServerSide.Resources;
using Yotalab.PlanningPoker.Grains.Interfaces.Models;

namespace Yotalab.PlanningPoker.BlazorServerSide.Pages.Components
{
  partial class BulletinEditor
  {
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

    private MudDropContainer<BulletinItemViewModel> itemsReorderContainer;

    private BulletinViewModel bulletinEditViewModels = new BulletinViewModel(null);

    private Bulletin bulletin;

    [Parameter]
    public Bulletin Bulletin
    {
      get
      {
        return this.bulletin;
      }

      set
      {
        if (this.bulletin != value)
        {
          this.bulletin = value;
          this.bulletinEditViewModels = new BulletinViewModel(this.bulletin);
        }
      }
    }

    private bool isEditMode = false;

    [Parameter]
    public bool IsEditMode
    {
      get
      {
        return this.isEditMode;
      }

      set
      {
        if (this.isEditMode != value)
        {
          this.isEditMode = value;
          if (this.isEditMode)
            this.bulletinEditViewModels = new BulletinViewModel(this.bulletin);
        }
      }
    }

    [Inject]
    private ISnackbar Snackbar { get; set; }

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
    private void HandleRemoveVoteClick(BulletinItemViewModel viewModel)
    {
      this.Snackbar.Clear();
      this.bulletinEditViewModels.Remove(viewModel);
      this.RefreshContainer();
    }

    private void RefreshContainer()
    {
      this.StateHasChanged();
      this.itemsReorderContainer.Refresh();
    }

    private void HandleAddVoteClick()
    {
      this.Snackbar.Clear();
      this.bulletinEditViewModels.AddNumber();
      this.RefreshContainer();
    }

    private void HandleAddEmojiVoteClick(string emoji)
    {
      this.Snackbar.Clear();
      if (!this.bulletinEditViewModels.AddEmoji(emoji))
        this.Snackbar.Add(string.Format(UIResources.BulletinItemAlreadyExists, emoji), Severity.Warning);
      else
        this.RefreshContainer();
    }

    private void HandleVoteValueChange(BulletinItemViewModel viewModel, ChangeEventArgs e)
    {
      this.Snackbar.Clear();
      var value = e.Value as string;
      if (!this.bulletinEditViewModels.UpdateVote(viewModel, value))
        this.Snackbar.Add(viewModel.ErrorMessage, Severity.Error);

      this.RefreshContainer();
    }

    private void HandleVoteDropped(MudItemDropInfo<BulletinItemViewModel> dropItem)
    {
      if (this.itemsReorderContainer.HasTransactionIndexChanged() && dropItem.IndexInZone != dropItem.Item.Order)
        this.bulletinEditViewModels.UpdateOrder(dropItem);
    }
  }

  public class BulletinViewModel
  {
    private readonly Bulletin bulletin;

    private readonly List<BulletinItemViewModel> bulletinItemsViewModel = new List<BulletinItemViewModel>();

    public IReadOnlyCollection<BulletinItemViewModel> Items => this.bulletinItemsViewModel.OrderBy(i => i.Order).ToList();

    public BulletinViewModel(Bulletin bulletin)
    {
      this.bulletin = bulletin;
      if (bulletin != null)
      {
        var bulletinItems = this.bulletin.Where(i => !i.Vote.IsUnset).ToArray();
        for (int i = 0; i < bulletinItems.Length; i++)
          this.bulletinItemsViewModel.Add(new BulletinItemViewModel(bulletinItems[i], i));
      }
    }

    public void UpdateBulletin()
    {
      this.bulletin.Clear();
      foreach (var item in this.bulletinItemsViewModel.OrderBy(i => i.Order))
        this.bulletin.Add(new BulletinItem(item.Vote));
    }

    public void UpdateOrder(MudItemDropInfo<BulletinItemViewModel> dropItem)
    {
      this.bulletinItemsViewModel.UpdateOrder(dropItem, item => item.Order);
      this.UpdateBulletin();
    }

    public void Remove(BulletinItemViewModel viewModel)
    {
      this.bulletinItemsViewModel.Remove(viewModel);
      var items = this.bulletin.Where(i => i.Vote.Equals(viewModel.Vote)).ToList();
      items.ForEach(i => this.bulletin.Remove(i));
    }

    public bool UpdateVote(BulletinItemViewModel viewModel, string value)
    {
      if (string.IsNullOrWhiteSpace(value))
      {
        viewModel.SetError(UIResources.BulletinItemIsEmpty);
        return false;
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

      if (this.bulletin.Any(i => i.Vote.Equals(newVote) && !i.Vote.Equals(viewModel.Vote)))
      {
        viewModel.SetError(string.Format(UIResources.BulletinItemAlreadyExists, newVote.Value));
        return false;
      }

      viewModel.UpdateVote(newVote);
      this.UpdateBulletin();

      return true;
    }

    public bool AddEmoji(string emoji)
    {
      var emojiVotes = this.bulletin.Where(i => i.Vote.Value?.Equals(emoji) == true);
      if (!emojiVotes.Any())
      {
        var newItem = new BulletinItem(new Vote(emoji));
        this.bulletin.Add(newItem);
        this.bulletinItemsViewModel.Add(new BulletinItemViewModel(newItem, this.bulletin.Count));
        return true;
      }

      return false;
    }

    public void AddNumber()
    {
      var numbers = this.bulletin.Where(i => i.Vote.IsNumber);
      BulletinItem newItem;
      if (numbers.Any())
        newItem = new BulletinItem(new Vote(numbers.Max(i => double.Parse(i.Vote.Value)) + 10));
      else
        newItem = new BulletinItem(new Vote(0));

      this.bulletin.Add(newItem);
      this.bulletinItemsViewModel.Add(new BulletinItemViewModel(newItem, this.bulletin.Count));
    }
  }

  public class BulletinItemViewModel
  {
    public BulletinItemViewModel(BulletinItem item, int order)
    {
      this.Item = item ?? throw new ArgumentNullException(nameof(item));
      this.Order = order;
    }

    public int Order { get; set; }

    public Vote Vote => this.Item.Vote;

    private BulletinItem Item { get; set; }

    public bool IsInvalid { get; private set; }

    public string ErrorMessage { get; private set; }

    public void SetError(string message)
    {
      this.IsInvalid = true;
      this.ErrorMessage = message;
    }

    public void UpdateVote(Vote newVote)
    {
      this.Item = new BulletinItem(newVote);
      this.IsInvalid = false;
      this.ErrorMessage = null;
    }
  }
}
