using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using Yotalab.PlanningPoker.BlazorServerSide.Services.Args;
using Yotalab.PlanningPoker.Grains.Interfaces.Models;

namespace Yotalab.PlanningPoker.BlazorServerSide.Pages.Components
{
  public partial class EditSessionDialog : IDisposable
  {
    private bool formInvalid;
    private EditContext editContext;

    [CascadingParameter] MudDialogInstance MudDialog { get; set; }

    [Parameter]
    public string Id { get; set; }

    [Parameter]
    public EditSessionArgs EditArgs { get; set; }

    [Parameter]
    public EventCallback<EditSessionArgs> OnConfirm { get; set; }

    public static void Show(IDialogService dialogService, string title, EditSessionArgs editArgs, EventCallback<EditSessionArgs> onConfirm)
    {
      var parameters = new DialogParameters();
      parameters.Add(nameof(EditArgs), editArgs);
      parameters.Add(nameof(OnConfirm), onConfirm);

      var options = new DialogOptions()
      {
        CloseButton = true,
        MaxWidth = MaxWidth.Small,
        FullWidth = true
      };
      dialogService.Show<EditSessionDialog>(title, parameters, options);
    }

    protected override void OnParametersSet()
    {
      base.OnParametersSet();

      if (this.editContext != null)
        this.editContext.OnFieldChanged -= this.HandleFieldChanged;

      if (this.EditArgs != null)
      {
        this.editContext = new EditContext(this.EditArgs);
        this.editContext.OnFieldChanged += this.HandleFieldChanged;
        this.formInvalid = !this.editContext.Validate();
      }
      else
      {
        this.editContext = null;
      }
    }

    private void HandleFieldChanged(object sender, FieldChangedEventArgs e)
    {
      this.formInvalid = !this.editContext.Validate();
      StateHasChanged();
    }

    private void HandleCheckVote(Vote vote)
    {
      if (this.EditArgs.Bulletin.IsEnabled(vote))
        this.EditArgs.Bulletin.Disable(vote);
      else
        this.EditArgs.Bulletin.Enable(vote);
    }

    private async Task ConfirmAsync()
    {
      if (!this.editContext.Validate())
        return;

      await this.OnConfirm.InvokeAsync(this.EditArgs);
      this.MudDialog.Close(DialogResult.Ok(true));
    }

    private void Cancel()
    {
      this.MudDialog.Cancel();
    }

    public void Dispose()
    {
      if (this.editContext != null)
        this.editContext.OnFieldChanged -= this.HandleFieldChanged;
    }
  }
}
