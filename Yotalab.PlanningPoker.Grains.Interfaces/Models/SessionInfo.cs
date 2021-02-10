using System;
using Orleans.Concurrency;

namespace Yotalab.PlanningPoker.Grains.Interfaces.Models
{
  /// <summary>
  /// Информация о сессии планирования.
  /// </summary>
  [Immutable]
  public class SessionInfo
  {
    /// <summary>
    /// Получить или установить идентификатор сессии.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Получить или установить имя сессии.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Получить или установить состояние сессии.
    /// </summary>
    public SessionProcessingState ProcessingState { get; set; }

    /// <summary>
    /// Получить или установить идентификатор модератора сессии.
    /// </summary>
    public Guid ModeratorId { get; set; }

    /// <summary>
    /// Получить или установить количество участников сессии планирования.
    /// </summary>
    public int ParticipantsCount { get; set; }
  }
}
