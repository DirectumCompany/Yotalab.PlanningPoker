namespace Yotalab.PlanningPoker.Grains.Interfaces.Models.Args
{
  /// <summary>
  /// Аргументы изменения информации о сессии.
  /// </summary>
  public class ChangeSessionInfoArgs
  {
    /// <summary>
    /// Получить или установить имя сессии.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Получить или установить признак необходимости автоматически остановить голосование, когда все участники проголосовали.
    /// </summary>
    public bool AutoStop { get; set; }
  }
}
