using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Yotalab.PlanningPoker.BlazorServerSide.Resources;
using Yotalab.PlanningPoker.BlazorServerSide.Services;
using Yotalab.PlanningPoker.BlazorServerSide.Services.DTO;
using Yotalab.PlanningPoker.BlazorServerSide.Shared;
using Yotalab.PlanningPoker.Grains.Interfaces.Models;
using MudBlazorColor = MudBlazor.Color;
using MudBlazorIcons = MudBlazor.Icons;

namespace Yotalab.PlanningPoker.BlazorServerSide.Pages.Components
{
  public partial class ParticipantMenu
  {
    private List<DropDownMenuItem> contextMenuItems = new();

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> InputAttributes { get; set; }

    [Inject]
    private ParticipantsService Service { get; set; }

    [Inject]
    private SessionService SessionService { get; set; }

    [CascadingParameter]
    public SessionInfo Session { get; set; }

    [Parameter]
    public ParticipantInfoDTO Participant { get; set; }

    [Parameter]
    public Guid ParticipantId { get; set; }


    protected override void OnParametersSet()
    {
      this.contextMenuItems = this.BuildContextMenu();
    }

    private List<DropDownMenuItem> BuildContextMenu()
    {
      var contextMenuItems = new List<DropDownMenuItem>();
      // Строим меню модератора.
      if (this.Session.ModeratorIds.Contains(this.ParticipantId))
      {
        // Меню на другом участнике сессии.
        if (this.ParticipantId != this.Participant.Id)
        {
          if (this.Session.ModeratorIds.Contains(this.Participant.Id))
          {
            contextMenuItems.Add(new DropDownMenuItem()
            {
              OnClickAsync = (args) => this.SessionService.RemoveModerator(this.Session.Id, this.Participant.Id),
              Icon = MudBlazorIcons.Material.Filled.RemoveModerator,
              IconColor = MudBlazorColor.Default,
              Title = UIResources.RemoveModeratorButton
            });
          }
          else
          {
            contextMenuItems.Add(new DropDownMenuItem()
            {
              OnClickAsync = (args) => this.SessionService.AddModerator(this.Session.Id, this.Participant.Id),
              Icon = MudBlazorIcons.Material.Filled.AddModerator,
              IconColor = MudBlazorColor.Warning,
              Title = UIResources.AddModeratorButton
            });
          }

          contextMenuItems.Add(new DropDownMenuItem()
          {
            OnClickAsync = (args) => this.Service.KickAsync(this.Session.Id, this.Participant.Id, this.ParticipantId),
            Icon = MudBlazorIcons.Material.Filled.PersonRemove,
            IconColor = MudBlazorColor.Error,
            Title = UIResources.ExcludeParticipantButton
          });
        }
        else // Меню на себе.
        {
          if (this.Session.ModeratorIds.Length > 1)
          {
            contextMenuItems.Add(new DropDownMenuItem()
            {
              OnClickAsync = (args) => this.Service.LeaveAsync(this.Session.Id, this.ParticipantId),
              Icon = MudBlazorIcons.Material.Filled.Close,
              IconColor = MudBlazorColor.Error,
              Title = UIResources.LeaveSessionButton
            });
          }
        }
      }
      else // Меню участника.
      {
        // Меню на себе.
        if (this.ParticipantId == this.Participant.Id)
        {
          contextMenuItems.Add(new DropDownMenuItem()
          {
            OnClickAsync = (args) => this.Service.LeaveAsync(this.Session.Id, this.ParticipantId),
            Icon = MudBlazorIcons.Material.Filled.Close,
            IconColor = MudBlazorColor.Error,
            Title = UIResources.LeaveSessionButton
          });
        }
        else // Меню на другом участнике сессии.
        {
          // Нет элементов.
        }
      }

      return contextMenuItems;
    }
  }
}
