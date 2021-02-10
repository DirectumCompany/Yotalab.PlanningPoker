using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yotalab.PlanningPoker.BlazorServerSide.Pages.Components;
using Yotalab.PlanningPoker.BlazorServerSide.Services;
using Yotalab.PlanningPoker.Grains.Interfaces.Models;

namespace Yotalab.PlanningPoker.BlazorServerSide.Pages
{
  public partial class Sessions : AuthorizedOwningComponentBase<SessionService>
  {
    private Guid participantId;
    private string newSessionName;
    private List<SessionInfo> sessions;

    protected override async Task OnInitializedAsync()
    {
      // Обязательно надо вызвать базовый класс, в нем происходит инициализация UserManager и др.
      await base.OnInitializedAsync();

      this.participantId = this.ParticipantId;
      this.sessions = new List<SessionInfo>(await this.Service.ListAsync(this.participantId));
    }

    private async Task CreateSessionAsync()
    {
      if (string.IsNullOrWhiteSpace(this.newSessionName))
        return;

      var result = await this.Service.CreateAsync(this.newSessionName, this.participantId);
      this.sessions.Add(result);
      this.newSessionName = string.Empty;
    }
  }
}
