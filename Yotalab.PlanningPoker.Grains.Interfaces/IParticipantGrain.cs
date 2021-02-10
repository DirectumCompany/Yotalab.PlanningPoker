using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Orleans;
using Yotalab.PlanningPoker.Grains.Interfaces.Models;

namespace Yotalab.PlanningPoker.Grains.Interfaces
{
  /// <summary>
  /// Грейн участника сессии планирования.
  /// </summary>
  public interface IParticipantGrain : IGrainWithGuidKey
  {
    /// <summary>
    /// Получить информацию об участнике.
    /// </summary>
    /// <returns>Задача на получение информации об участнике.</returns>
    Task<ParticipantInfo> GetAsync();

    /// <summary>
    /// Присоединиться к сессии планирования.
    /// </summary>
    /// <param name="sessionId">Идентификатор сессии.</param>
    /// <returns>Задача на присоединение к сессии.</returns>
    Task Join(Guid sessionId);

    /// <summary>
    /// Отдать свой голос в сессии планирования.
    /// </summary>
    /// <param name="sessionId">Идентификатор сессии.</param>
    /// <param name="vote">Голос.</param>
    /// <returns>Задача на голосование.</returns>
    Task Vote(Guid sessionId, Vote vote);

    /// <summary>
    /// Получить сессии, в которых состоит участник.
    /// </summary>
    /// <returns>Задача на получение списка сессий участника.</returns>
    // TODO: Подумать над тем, чтобы вынести в отдельный Grain IParticipantSessions.
    Task<ImmutableArray<ISessionGrain>> Sessions();

    /// <summary>
    /// Изменить информацию об участнике.
    /// </summary>
    /// <param name="newName">Новое имя участника.</param>
    /// <param name="newAvatarUrl">Новый URL до аватарки.</param>
    /// <returns>Задача на изменение аватарки.</returns>
    Task ChangeInfo(string newName, string newAvatarUrl);
  }
}
