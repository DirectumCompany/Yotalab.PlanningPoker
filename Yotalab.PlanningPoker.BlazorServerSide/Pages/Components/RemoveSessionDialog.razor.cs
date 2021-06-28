using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Yotalab.PlanningPoker.Grains.Interfaces.Models;

namespace Yotalab.PlanningPoker.BlazorServerSide.Pages.Components
{
  public partial class RemoveSessionDialog
  {
    [CascadingParameter] MudDialogInstance MudDialog { get; set; }

    [Parameter]
    public SessionInfo Session { get; set; }

    [Parameter]
    public EventCallback<Guid> OnConfirm { get; set; }

    public static void Show(IDialogService dialogService, SessionInfo session, EventCallback<Guid> onConfirm)
    {
      var parameters = new DialogParameters();
      parameters.Add(nameof(Session), session);
      parameters.Add(nameof(OnConfirm), onConfirm);

      var options = new DialogOptions()
      {
        CloseButton = true,
        MaxWidth = MaxWidth.Small,
        FullWidth = true
      };
      dialogService.Show<RemoveSessionDialog>("Удалить сессию", parameters, options);
    }

    private async Task ConfirmAsync()
    {
      await this.OnConfirm.InvokeAsync(this.Session.Id);
      this.MudDialog.Close(DialogResult.Ok(true));
    }

    private void Cancel()
    {
      this.MudDialog.Cancel();
    }
  }
}
