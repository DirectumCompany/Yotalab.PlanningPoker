using System;
using Orleans;

namespace Yotalab.PlanningPoker.Grains.Interfaces.Models.Notifications
{
  /// <summary>
  /// Уведомление о голосе участника.
  /// </summary>
  [Immutable]
  [GenerateSerializer]
  public class VoteNotification
  {
    public VoteNotification(Guid sessionId, Guid participantId, Vote vote)
    {
      this.SessionId = sessionId;
      this.ParticipantId = participantId;
      this.Vote = vote;
    }

    /// <summary>
    /// Получить идентификатор сессии планирования.
    /// </summary>
    [Id(0)]
    public Guid SessionId { get; }

    /// <summary>
    /// Получить идентификатор участника.
    /// </summary>
    [Id(1)]
    public Guid ParticipantId { get; }

    /// <summary>
    /// Получить голос участника.
    /// </summary>
    [Id(2)]
    public Vote Vote { get; }
  }
}
