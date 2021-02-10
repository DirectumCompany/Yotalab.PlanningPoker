using System;
using Orleans.Concurrency;

namespace Yotalab.PlanningPoker.Grains.Interfaces.Models
{
  /// <summary>
  /// Информация об участнике сесии планирования.
  /// </summary>
  [Immutable]
  public class ParticipantInfo
  {
    /// <summary>
    /// Получить или установить идентификатор участника.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Получить или установить имя участника.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Получить или установить Url аватарки участника.
    /// </summary>
    public string AvatarUrl { get; set; }
  }
}