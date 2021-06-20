using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Yotalab.PlanningPoker.BlazorServerSide.Services.Args;
using Yotalab.PlanningPoker.BlazorServerSide.Shared;

namespace Yotalab.PlanningPoker.BlazorServerSide.Pages.Components
{
  public partial class EditSessionModal : ComponentBase, IDisposable
  {
    private bool formInvalid;
    private EditContext editContext;

    [Parameter]
    public string Id { get; set; }

    [Parameter]
    public string Title { get; set; }

    [Parameter]
    public EditSessionArgs EditArgs { get; set; }

    [Parameter]
    public EventCallback<EditSessionArgs> OnConfirm { get; set; }

    protected override void OnParametersSet()
    {
      base.OnParametersSet();

      if (this.editContext != null)
        this.editContext.OnFieldChanged -= this.HandleFieldChanged;

      if (this.EditArgs != null)
      {
        this.editContext = new EditContext(this.EditArgs);
        this.editContext.SetFieldCssClassProvider(new Bootstrap5FieldClassProvider());
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

    private Task ConfirmAsync()
    {
      if (!this.editContext.Validate())
        return Task.CompletedTask;

      return this.OnConfirm.InvokeAsync(this.EditArgs);
    }

    public void Dispose()
    {
      if (this.editContext != null)
        this.editContext.OnFieldChanged -= this.HandleFieldChanged;
    }
  }
}
