using Orleans.Concurrency;

namespace Yotalab.PlanningPoker.Grains.Interfaces.Models.Notifications
{
  /// <summary>
  /// Уведомление об изменении одного участника.
  /// </summary>
  [Immutable]
  public class ParticipantChangedNotification
  {
    public ParticipantChangedNotification(ParticipantInfo changedInfo)
    {
      this.ChangedInfo = changedInfo;
    }

    /// <summary>
    /// Получить измененную информацию участника.
    /// </summary>
    public ParticipantInfo ChangedInfo { get; }
  }
}
