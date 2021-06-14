using System;
using System.ComponentModel.DataAnnotations;

namespace Yotalab.PlanningPoker.BlazorServerSide.Services.Args
{
  /// <summary>
  /// Аргументы события редактирования сессии планирования.
  /// </summary>
  public class EditSessionArgs
  {
    /// <summary>
    /// Идентификатор сессии планирования.
    /// </summary>
    public Guid SessionId { get; set; }

    /// <summary>
    /// Имя сессии.
    /// </summary>
    [Required(ErrorMessage = "Имя сессии не может быть пустым!")]
    public string Name { get; set; }

    /// <summary>
    /// Получить или установить признак необходимости автоматически остановить голосование, когда все участники проголосовали.
    /// </summary>
    public bool AutoStop { get; set; }
  }
}
