using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Orleans;
using Yotalab.PlanningPoker.Grains.Interfaces.Models;
using Yotalab.PlanningPoker.Grains.Interfaces.Models.Args;

namespace Yotalab.PlanningPoker.Grains.Interfaces
{
  /// <summary>
  /// Грейн сессии планирования.
  /// </summary>
  public interface ISessionGrain : IGrainWithGuidKey
  {
    /// <summary>
    /// Создать сессию.
    /// </summary>
    /// <param name="name">Имя сессии.</param>
    /// <param name="moderator">Модератор сессии.</param>
    /// <returns>Задача на создание сессии.</returns>
    Task CreateAsync(string name, IParticipantGrain moderator);

    /// <summary>
    /// Получить статус сессии.
    /// </summary>
    /// <returns>Задача на получение статуса сессии.</returns>
    Task<SessionInfo> StatusAsync();

    /// <summary>
    /// Перевести сессию в начальное состояние.
    /// </summary>
    /// <param name="initiatorId">Инициатор исключения.</param>
    /// <returns>Задача на перевод сессии в начальное состояние.</returns>
    Task ResetAsync(Guid initiatorId);

    /// <summary>
    /// Перевести сессию в начальное состояние.
    /// </summary>
    /// <param name="initiatorId">Инициатор исключения.</param>
    /// <param name="startImmediately">После сброса сразу стартовать сессию.</param>
    /// <returns>Задача на сброс и начало сессии.</returns>
    Task ResetAsync(Guid initiatorId, bool startImmediately);

    /// <summary>
    /// Начать сессию планирования.
    /// </summary>
    /// <param name="initiatorId">Инициатор исключения.</param>
    /// <returns>Задача на старт сессии.</returns>
    Task StartAsync(Guid initiatorId);

    /// <summary>
    /// Остановить сессию планирования.
    /// </summary>
    /// <param name="initiatorId">Инициатор исключения.</param>
    /// <returns>Задача на остановку планирования.</returns>
    Task StopAsync(Guid initiatorId);

    /// <summary>
    /// Завершить сессию планирования.
    /// </summary>
    /// <param name="initiatorId">Инициатор исключения.</param>
    /// <returns>Задача на завершение сессии.</returns>
    Task FinishAsync(Guid initiatorId);

    /// <summary>
    /// Получить голоса участников.
    /// </summary>
    /// <returns>Список голосов участников.</returns>
    Task<ImmutableArray<ParticipantVote>> ParticipantVotes();

    /// <summary>
    /// Принять голос участника.
    /// </summary>
    /// <param name="participantId">Участник сессии.</param>
    /// <param name="vote">Голос.</param>
    /// <returns>Задача на прием голоса.</returns>
    Task AcceptVote(Guid participantId, Vote vote);

    /// <summary>
    /// Войти в сессию пользователем.
    /// </summary>
    /// <param name="participantId">Идентификатор участника.</param>
    /// <returns>Задача на вход в сессию.</returns>
    Task Enter(Guid participantId);

    /// <summary>
    /// Выйти пользователем из сессии.
    /// </summary>
    /// <param name="participantId">Идентификатор участника.</param>
    /// <returns>Задача на выход из сессии.</returns>
    Task Exit(Guid participantId);

    /// <summary>
    /// Исключить пользователя из сессии.
    /// </summary>
    /// <param name="participantId">Идентификатор участника.</param>
    /// <param name="initiatorId">Инициатор исключения.</param>
    /// <returns>Задача на исключение из сессии.</returns>
    Task Kick(Guid participantId, Guid initiatorId);

    /// <summary>
    /// Добавить модератора сессии.
    /// </summary>
    /// <param name="participantId">Идентификатор участника.</param>
    /// <returns>Задача на добавление модератора.</returns>
    Task AddModerator(Guid participantId);

    /// <summary>
    /// Удалить модератора сессии.
    /// </summary>
    /// <param name="participantId">Идентификатор участника.</param>
    /// <returns>Задача на удаление модератора.</returns>
    Task RemoveModerator(Guid participantId);

    /// <summary>
    /// Изменить информацию о сессии.
    /// </summary>
    /// <param name="args">Аргументы изменения информации о сессии.</param>
    /// <returns>Задача на изменение настроек сессии.</returns>
    Task ChangeInfo(ChangeSessionInfoArgs args);
  }
}
