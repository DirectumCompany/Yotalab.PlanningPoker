using System;
using System.ComponentModel.DataAnnotations;
using Yotalab.PlanningPoker.Grains.Interfaces.Models;

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
    /// Получить или установить бюллетень голосования.
    /// </summary>
    public Bulletin Bulletin { get; set; }
  }
}
