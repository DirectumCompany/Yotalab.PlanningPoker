using System.Collections.Generic;
using System;
using Yotalab.PlanningPoker.Grains.Interfaces.Models;

namespace Yotalab.PlanningPoker.Api.Models
{
  public class ObsoleteSessionGrainState
  {
    /// <summary>
    /// Получить или установить имя сессии.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Получить или установить признак необходимости автоматически остановить голосование, когда все участники проголосовали.
    /// </summary>
    public bool AutoStop { get; set; }

    /// <summary>
    /// Получить или установить список модераторов сессии.
    /// </summary>
    public HashSet<Guid> ModeratorIds { get; set; }

    /// <summary>
    /// Получить или установить список наблюдателей сессии.
    /// </summary>
    public HashSet<Guid> ObserverIds { get; set; }

    /// <summary>
    /// Получить или установить состояние голосования.
    /// </summary>
    public SessionProcessingState ProcessingState { get; set; }

    /// <summary>
    /// Получить или установить состояние голосов.
    /// </summary>
    public Dictionary<Guid, Vote> ParticipantVotes { get; set; }

    /// <summary>
    /// Получить или установить бюллетень голосования.
    /// </summary>
    public ObsoleteBulletin Bulletin { get; set; }
  }
}
