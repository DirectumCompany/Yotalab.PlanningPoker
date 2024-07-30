using System;
using System.Collections.Immutable;
using Orleans;

namespace Yotalab.PlanningPoker.Grains.Interfaces.Models
{
  /// <summary>
  /// Информация о сессии планирования.
  /// </summary>
  [Immutable]
  [GenerateSerializer]
  public class SessionInfo
  {
    /// <summary>
    /// Получить или установить идентификатор сессии.
    /// </summary>
    [Id(0)]
    public Guid Id { get; set; }

    /// <summary>
    /// Получить или установить имя сессии.
    /// </summary>
    [Id(1)]
    public string Name { get; set; }

    /// <summary>
    /// Получить или установить признак необходимости автоматически остановить голосование, когда все участники проголосовали.
    /// </summary>
    [Id(2)]
    public bool AutoStop { get; set; }

    /// <summary>
    /// Получить или установить признак того, что сессия инициализирована.
    /// </summary>
    [Id(3)]
    public bool IsInitialized { get; set; }

    /// <summary>
    /// Получить или установить состояние сессии.
    /// </summary>
    [Id(4)]
    public SessionProcessingState ProcessingState { get; set; }

    /// <summary>
    /// Получить или установить список модераторов сессии.
    /// </summary>
    [Id(5)]
    public ImmutableArray<Guid> ModeratorIds { get; set; }

    /// <summary>
    /// Получить или установить список наблюдателей сессии.
    /// </summary>
    [Id(6)]
    public ImmutableArray<Guid> ObserverIds { get; set; }

    /// <summary>
    /// Получить или установить количество участников сессии планирования.
    /// </summary>
    [Id(7)]
    public int ParticipantsCount { get; set; }
  }
}
