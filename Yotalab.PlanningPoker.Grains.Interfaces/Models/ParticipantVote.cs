using System;
using Orleans.Concurrency;

namespace Yotalab.PlanningPoker.Grains.Interfaces.Models
{
  /// <summary>
  /// Информация об отданом голосе участника сессии планирования.
  /// </summary>
  [Immutable]
  public class ParticipantVote
  {
    /// <summary>
    /// Получить или установить идентификатор участника сессии.
    /// </summary>
    public Guid ParticipantId { get; set; }

    /// <summary>
    /// Получить или установить голос участника.
    /// </summary>
    public Vote Vote { get; set; }
  }
}