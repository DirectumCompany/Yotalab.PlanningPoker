using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yotalab.PlanningPoker.BlazorServerSide.Pages.Components;
using Yotalab.PlanningPoker.BlazorServerSide.Services;
using Yotalab.PlanningPoker.BlazorServerSide.Services.Args;
using Yotalab.PlanningPoker.Grains.Interfaces.Models;

namespace Yotalab.PlanningPoker.BlazorServerSide.Pages
{
  public partial class Sessions : AuthorizedOwningComponentBase<SessionService>
  {
    private Guid participantId;
    private List<SessionInfo> sessions;
    private EditSessionArgs newSessionArgs;

    protected override async Task OnInitializedAsync()
    {
      // Обязательно надо вызвать базовый класс, в нем происходит инициализация UserManager и др.
      await base.OnInitializedAsync();

      this.participantId = this.ParticipantId;
      this.sessions = new List<SessionInfo>(await this.Service.ListAsync(this.participantId));
    }

    private async Task CreateSessionAsync(EditSessionArgs args)
    {
      var result = await this.Service.CreateAsync(args.Name, this.participantId);
      this.sessions.Add(result);
    }

    private void OnShowCreationModal()
    {
      this.newSessionArgs = new EditSessionArgs()
      {
        Name = "Новая сессия"
      };
    }
  }
}
