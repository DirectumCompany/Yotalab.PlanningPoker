using System;
using System.Collections.Generic;
using System.Linq;
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
    private SessionInfo sessionToDelete;

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

    private async Task RemoveSessionAsync(Guid sessionId)
    {
      try
      {
        var session = this.sessions.Single(s => s.Id == sessionId);
        await this.Service.Remove(sessionId, this.participantId);
        this.sessions.Remove(session);
      }
      finally
      {
        this.sessionToDelete = null;
      }
    }

    private void ShowRemoveSessionConfirmation(Guid sessionId)
    {
      this.sessionToDelete = this.sessions.Single(s => s.Id == sessionId);
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
