using System.Collections.Generic;

namespace Yotalab.PlanningPoker.Grains.Interfaces.Models
{
  /// <summary>
  /// Сравниватель элементов бюллетени.
  /// Сравнивает по голосу, игнорируя признак недоступности.
  /// </summary>
  internal class BulletinItemComparer : IEqualityComparer<BulletinItem>
  {
    #region IEqualityComparer<BulletinItem>

    public bool Equals(BulletinItem x, BulletinItem y)
    {
      if (x == null && y == null)
        return true;
      else if (x == null || y == null)
        return false;

      return x.Vote.Equals(y.Vote);
    }

    public int GetHashCode(BulletinItem obj)
    {
      return obj.Vote.GetHashCode();
    }

    #endregion
  }
}
