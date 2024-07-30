using System;
using Orleans;
using Orleans.Concurrency;

namespace Yotalab.PlanningPoker.Grains.Interfaces.Models
{
  /// <summary>
  /// Информация об отданом голосе участника сессии планирования.
  /// </summary>
  [Immutable]
  [GenerateSerializer]
  public class ParticipantVote
  {
    /// <summary>
    /// Получить или установить идентификатор участника сессии.
    /// </summary>
    [Id(0)]
    public Guid ParticipantId { get; set; }

    /// <summary>
    /// Получить или установить голос участника.
    /// </summary>
    [Id(1)]
    public Vote Vote { get; set; }
  }
}