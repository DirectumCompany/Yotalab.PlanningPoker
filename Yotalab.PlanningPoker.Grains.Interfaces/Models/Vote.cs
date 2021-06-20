using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Orleans.Concurrency;

namespace Yotalab.PlanningPoker.Grains.Interfaces.Models
{
  /// <summary>
  /// Голос участника сессии планирования.
  /// </summary>
  [Immutable]
  public class Vote : IEquatable<Vote>
  {
    public static readonly Vote Unset = new Vote(null);
    public static readonly Vote Zero = new Vote(0);
    public static readonly Vote Half = new Vote(0.5);
    public static readonly Vote One = new Vote(1);
    public static readonly Vote Two = new Vote(2);
    public static readonly Vote Three = new Vote(3);
    public static readonly Vote Five = new Vote(5);
    public static readonly Vote Eight = new Vote(8);
    public static readonly Vote Thirteen = new Vote(13);
    public static readonly Vote Twenty = new Vote(20);
    public static readonly Vote Forty = new Vote(40);
    public static readonly Vote Hundred = new Vote(100);
    public static readonly Vote IDontKnown = new Vote("❔");
    public static readonly Vote Coffee = new Vote("☕");

    /// <summary>
    /// Получить значение голоса.
    /// </summary>
    public string Value { get; private set; }

    /// <summary>
    /// Получить признак того, что голос является числовой оценкой.
    /// </summary>
    public bool IsNumber => double.TryParse(this.Value, out _);

    /// <summary>
    /// Получить признак того, что голос не установлен.
    /// </summary>
    public bool IsUnset => Vote.Unset.Equals(this);

    /// <summary>
    /// Получить все возможные значения голосов.
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<Vote> GetAll()
    {
      var fields = typeof(Vote).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
      return fields.Select(f => f.GetValue(null)).OfType<Vote>();
    }

    #region IEquatable<Vote>

    public bool Equals(Vote other)
    {
      if (other == null) return false;
      return this.Value == other.Value;
    }

    #endregion

    #region Конструкторы

    private Vote(string value)
    {
      this.Value = value;
    }

    private Vote(int value)
    {
      this.Value = value.ToString();
    }

    private Vote(double value)
    {
      this.Value = value.ToString();
    }

    private Vote()
    {
      // Требует десериализатор.
    }

    #endregion
  }
}
