using System;
using Orleans.Concurrency;

namespace Yotalab.PlanningPoker.Grains.Interfaces.Models.Notifications
{
  /// <summary>
  /// Уведомление о голосе участника.
  /// </summary>
  [Immutable]
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
    public Guid SessionId { get; }

    /// <summary>
    /// Получить идентификатор участника.
    /// </summary>
    public Guid ParticipantId { get; }

    /// <summary>
    /// Получить голос участника.
    /// </summary>
    public Vote Vote { get; }
  }
}
