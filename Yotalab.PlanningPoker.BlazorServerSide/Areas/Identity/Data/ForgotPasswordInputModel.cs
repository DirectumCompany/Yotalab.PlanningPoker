using System.ComponentModel.DataAnnotations;
using Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Resources;

namespace Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Data
{
  public class ForgotPasswordInputModel
  {
    [Required(ErrorMessageResourceName = nameof(DataAnnotationResources.EmailRequired), ErrorMessageResourceType = typeof(DataAnnotationResources))]
    [EmailAddress(ErrorMessageResourceName = nameof(DataAnnotationResources.EmailInvalid), ErrorMessageResourceType = typeof(DataAnnotationResources))]
    [Display(Name = nameof(DataAnnotationResources.EmailLabel), ResourceType = typeof(DataAnnotationResources))]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }
  }
}
