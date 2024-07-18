using Orleans;

namespace Yotalab.PlanningPoker.Grains.Interfaces.Models.Args
{
  /// <summary>
  /// Аргументы изменения информации о сессии.
  /// </summary>
  [GenerateSerializer]
  public class ChangeSessionInfoArgs
  {
    /// <summary>
    /// Получить или установить имя сессии.
    /// </summary>
    [Id(0)]
    public string Name { get; set; }

    /// <summary>
    /// Получить или установить признак необходимости автоматически остановить голосование, когда все участники проголосовали.
    /// </summary>
    [Id(1)]
    public bool AutoStop { get; set; }
  }
}
