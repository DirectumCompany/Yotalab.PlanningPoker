using Orleans;

namespace Yotalab.PlanningPoker.Grains.Interfaces.Models.Notifications
{
  /// <summary>
  /// Уведомление об изменении одного участника.
  /// </summary>
  [Immutable]
  [GenerateSerializer]
  public class ParticipantChangedNotification
  {
    public ParticipantChangedNotification(ParticipantInfo changedInfo)
    {
      this.ChangedInfo = changedInfo;
    }

    /// <summary>
    /// Получить измененную информацию участника.
    /// </summary>
    [Id(0)]
    public ParticipantInfo ChangedInfo { get; }
  }
}
