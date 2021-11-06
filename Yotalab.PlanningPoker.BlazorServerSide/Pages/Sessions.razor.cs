using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Yotalab.PlanningPoker.BlazorServerSide.Pages.Components;
using Yotalab.PlanningPoker.BlazorServerSide.Resources;
using Yotalab.PlanningPoker.BlazorServerSide.Services;
using Yotalab.PlanningPoker.BlazorServerSide.Services.Args;
using Yotalab.PlanningPoker.BlazorServerSide.Shared;
using Yotalab.PlanningPoker.Grains.Interfaces.Models;

namespace Yotalab.PlanningPoker.BlazorServerSide.Pages
{
  public partial class Sessions : AuthorizedOwningComponentBase<SessionService>
  {
    private Guid participantId;
    private List<SessionInfo> sessions;

    [Inject]
    private IDialogService DialogService { get; set; }

    protected override async Task OnInitializedAsync()
    {
      // Обязательно надо вызвать базовый класс, в нем происходит инициализация UserManager и др.
      await base.OnInitializedAsync();

      this.participantId = this.ParticipantId;
      this.sessions = new List<SessionInfo>(await this.Service.ListAsync(this.participantId));
    }

    private void OnRowClick(TableRowClickEventArgs<SessionInfo> args)
    {
      NavigationManager.NavigateTo($"sessions/{args.Item.Id}");
    }

    private IEnumerable<DropDownMenuItem> BuildRowContextMenu(SessionInfo session)
    {
      return new List<DropDownMenuItem>()
      {
        new()
        {
          Icon = Icons.Material.Filled.Delete,
          IconColor = Color.Error,
          Title = UIResources.RemoveSession,
          OnClick = (args) => this.ShowRemoveSessionConfirmation(session.Id)
        }
      };
    }

    private async Task CreateSessionAsync(EditSessionArgs args)
    {
      var result = await this.Service.CreateAsync(args.Name, this.participantId, args.AutoStop, args.Bulletin);
      this.sessions.Add(result);
    }

    private async Task RemoveSessionAsync(Guid sessionId)
    {
      var session = this.sessions.Single(s => s.Id == sessionId);
      await this.Service.Remove(sessionId, this.participantId);
      this.sessions.Remove(session);
    }

    private void ShowRemoveSessionConfirmation(Guid sessionId)
    {
      var sessionToDelete = this.sessions.Single(s => s.Id == sessionId);
      var onConfirm = new EventCallbackFactory().Create<Guid>(this, async () => await this.RemoveSessionAsync(sessionId));
      RemoveSessionDialog.Show(this.DialogService, sessionToDelete, onConfirm);
    }

    private void OnShowCreationModal()
    {
      var newSessionArgs = new EditSessionArgs()
      {
        Name = UIResources.NewSessionName,
        Bulletin = Bulletin.Default()
      };

      var onConfirm = new EventCallbackFactory().Create<EditSessionArgs>(this, this.CreateSessionAsync);
      EditSessionDialog.Show(this.DialogService, UIResources.CreateSession, newSessionArgs, onConfirm);
    }
  }
}
