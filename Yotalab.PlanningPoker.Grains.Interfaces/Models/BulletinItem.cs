namespace Yotalab.PlanningPoker.Grains.Interfaces.Models
{
  /// <summary>
  /// Элемент бюллетени.
  /// </summary>
  public class BulletinItem
  {
    /// <summary>
    /// Получить голос связанный с элементом бюллетени.
    /// </summary>
    public Vote Vote { get; private set; }

    /// <summary>
    /// Получить признак того, что элемент бюллетени недоступен.
    /// </summary>
    public bool IsDisabled { get; private set; }

    /// <summary>
    /// Сделать элемент бюллетени недоступным.
    /// </summary>
    internal void Disable()
    {
      this.IsDisabled = true;
    }

    /// <summary>
    /// Сделать элемент бюллетени доступным.
    /// </summary>
    internal void Enable()
    {
      this.IsDisabled = false;
    }

    public BulletinItem(Vote vote)
      : this(vote, false)
    {
    }

    public BulletinItem(Vote vote, bool disabled)
    {
      this.Vote = vote;
      this.IsDisabled = disabled;
    }

    private BulletinItem()
    {
      // Требует десериализатор.
    }
  }
}
