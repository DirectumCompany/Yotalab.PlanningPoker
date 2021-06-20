using System.Linq;
using Microsoft.AspNetCore.Components.Forms;

namespace Yotalab.PlanningPoker.BlazorServerSide.Shared
{
  public class Bootstrap5FieldClassProvider : FieldCssClassProvider
  {
    public override string GetFieldCssClass(EditContext editContext, in FieldIdentifier fieldIdentifier)
    {
      if (!editContext.IsModified(fieldIdentifier))
        return string.Empty;

      var propertyInfo = fieldIdentifier.Model?.GetType().GetProperty(fieldIdentifier.FieldName);
      if (propertyInfo != null && propertyInfo.PropertyType == typeof(bool))
        return string.Empty;

      var cssClassName = base.GetFieldCssClass(editContext, in fieldIdentifier);
      cssClassName = string.Join(' ', cssClassName.Split(' ').Select(token => token switch
      {
        "valid" => "is-valid",
        "invalid" => "is-invalid",
        _ => token
      }));

      return cssClassName;
    }
  }
}
