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
  public class Vote
  {
    public static readonly Vote Unset = new Vote();
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
    /// <returns>Перечисление всех возможных значений голосов.</returns>
    public static IEnumerable<Vote> GetAll()
    {
      var fields = typeof(Vote).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
      return fields.Select(f => f.GetValue(null)).OfType<Vote>();
    }

    protected static bool EqualOperator(Vote left, Vote right)
    {
      if (ReferenceEquals(left, null) ^ ReferenceEquals(right, null))
        return false;

      return ReferenceEquals(left, right) || left.Equals(right);
    }

    protected static bool NotEqualOperator(Vote left, Vote right)
    {
      return !EqualOperator(left, right);
    }

    protected IEnumerable<object> GetEqualityComponents()
    {
      yield return this.Value;
    }

    #region Базовый класс

    public override bool Equals(object obj)
    {
      if (obj == null || obj.GetType() != GetType())
        return false;

      var other = (Vote)obj;
      return this.GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
      return GetEqualityComponents()
        .Select(x => x != null ? x.GetHashCode() : 0)
        .Aggregate((x, y) => x ^ y);
    }

    #endregion

    #region Конструкторы

    public Vote(string value)
    {
      this.Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public Vote(int value)
    {
      this.Value = value.ToString();
    }

    public Vote(double value)
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
