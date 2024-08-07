﻿using System.Collections.Generic;
using System.Linq;
using Orleans;

namespace Yotalab.PlanningPoker.Grains.Interfaces.Models
{
  /// <summary>
  /// Бюллетень голосования, с возможными для выбора голосами.
  /// </summary>
  [Immutable]
  [GenerateSerializer]
  public class Bulletin
  {
    [Id(0)]
    private HashSet<BulletinItem> items;

    public IReadOnlySet<BulletinItem> Items => this.items;

    public BulletinItem Add(Vote vote)
    {
      var newItem = new BulletinItem(vote);
      this.items.Add(newItem);
      return newItem;
    }

    public BulletinItem Add(Vote vote, bool disabled)
    {
      var newItem = new BulletinItem(vote, disabled);
      this.items.Add(newItem);
      return newItem;
    }

    public void Remove(Vote vote) => this.items.Remove(new BulletinItem(vote));

    /// <summary>
    /// Бюллетень по-умолчанию из последовательности Фиббоначи, знака вопроса и чашки кофе.
    /// </summary>
    /// <returns>Бюлетень по-умолчанию.</returns>
    public static Bulletin Default()
    {
      return new Bulletin(Vote.GetAll());
    }

    /// <summary>
    /// Отметить голос, как недоступный для выбора.
    /// </summary>
    /// <param name="vote">Голос.</param>
    public void Disable(Vote vote)
    {
      foreach (var item in this.Items)
      {
        if (Equals(item.Vote, vote))
          item.Disable();
      }
    }

    /// <summary>
    /// Отметить голос, как доступный для выбора.
    /// </summary>
    /// <param name="vote">Голос.</param>
    public void Enable(Vote vote)
    {
      foreach (var item in this.Items)
      {
        if (Equals(item.Vote, vote))
          item.Enable();
      }
    }

    /// <summary>
    /// Проверить, доступен ли голос для выбора.
    /// </summary>
    /// <param name="vote">Голос.</param>
    /// <returns>True, если голос доступен для выбора, иначе - false.</returns>
    public bool IsEnabled(Vote vote)
    {
      return this.Items.Any(item => Equals(item.Vote, vote) && !item.IsDisabled);
    }

    public void Clear() => this.items.Clear();

    public Bulletin(IEnumerable<Vote> votes)
    {
      this.items = new HashSet<BulletinItem>(votes.Select(v => new BulletinItem(v)), new BulletinItemComparer());
    }

    public Bulletin(Bulletin bulletin)
    {
      this.items = new HashSet<BulletinItem>(bulletin.Items, new BulletinItemComparer());
    }

    internal Bulletin()
    {
      this.items = new HashSet<BulletinItem>(new BulletinItemComparer());
    }
  }
}
