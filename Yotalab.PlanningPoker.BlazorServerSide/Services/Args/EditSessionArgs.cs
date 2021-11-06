using System;
using System.ComponentModel.DataAnnotations;
using Yotalab.PlanningPoker.BlazorServerSide.Resources;
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
    [Required(ErrorMessageResourceName = nameof(UIResources.EditSessionDialogNameRequired), ErrorMessageResourceType = typeof(UIResources))]
    public string Name { get; set; }

    /// <summary>
    /// Получить или установить признак необходимости автоматически остановить голосование, когда все участники проголосовали.
    /// </summary>
    public bool AutoStop { get; set; }

    /// <summary>
    /// Получить или установить бюллетень голосования.
    /// </summary>
    public Bulletin Bulletin { get; set; }
  }
}
