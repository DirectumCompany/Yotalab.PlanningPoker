using System;
using Orleans;

namespace Yotalab.PlanningPoker.Grains.Interfaces.Models
{
  /// <summary>
  /// Информация об участнике сесии планирования.
  /// </summary>
  [Immutable]
  [GenerateSerializer]
  public class ParticipantInfo
  {
    /// <summary>
    /// Получить или установить идентификатор участника.
    /// </summary>
    [Id(0)]
    public Guid Id { get; set; }

    /// <summary>
    /// Получить или установить имя участника.
    /// </summary>
    [Id(1)]
    public string Name { get; set; }

    /// <summary>
    /// Получить или установить Url аватарки участника.
    /// </summary>
    [Id(2)]
    public string AvatarUrl { get; set; }
  }
}