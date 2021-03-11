using System;

namespace Yotalab.PlanningPoker.BlazorServerSide.Services.Args
{
  public class EditSessionArgs
  {
    public Guid SessionId { get; set; }

    public string Name { get; set; }
  }
}
