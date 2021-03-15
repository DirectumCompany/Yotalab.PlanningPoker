using System.Linq;
using Microsoft.AspNetCore.Components.Forms;

namespace Yotalab.PlanningPoker.BlazorServerSide.Shared
{
  public class Bootstrap5FieldClassProvider : FieldCssClassProvider
  {
    public override string GetFieldCssClass(EditContext editContext, in FieldIdentifier fieldIdentifier)
    {
      if (!editContext.IsModified())
        return string.Empty;

      var isValid = !editContext.GetValidationMessages(fieldIdentifier).Any();
      return isValid ? "is-valid" : "is-invalid";
    }
  }
}
