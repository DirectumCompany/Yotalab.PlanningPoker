using System;

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
    public string Name { get; set; }
  }
}
