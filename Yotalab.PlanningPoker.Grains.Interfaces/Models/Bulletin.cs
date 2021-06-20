using System.Collections.Generic;
using System.Linq;
using Orleans.Concurrency;

namespace Yotalab.PlanningPoker.Grains.Interfaces.Models
{
  /// <summary>
  /// Бюллетень голосования, с возможными для выбора голосами.
  /// </summary>
  [Immutable]
  public class Bulletin : HashSet<BulletinItem>
  {
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
      foreach (var item in this)
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
      foreach (var item in this)
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
      return this.Any(item => Equals(item.Vote, vote) && !item.IsDisabled);
    }

    public Bulletin(IEnumerable<Vote> votes)
      : base(votes.Select(v => new BulletinItem(v)), new BulletinItemComparer())
    {
    }

    public Bulletin(Bulletin bulletin)
      : base(bulletin, new BulletinItemComparer())
    {
    }

    private Bulletin()
    {
      // Требует десериализатор.
    }
  }
}
