using System.ComponentModel.DataAnnotations;
using Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Resources;

namespace Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Data
{
  public class LoginInputModel
  {
    [Required(ErrorMessageResourceName = nameof(DataAnnotationResources.EmailRequired), ErrorMessageResourceType = typeof(DataAnnotationResources))]
    [EmailAddress(ErrorMessageResourceName = nameof(DataAnnotationResources.EmailInvalid), ErrorMessageResourceType = typeof(DataAnnotationResources))]
    [Display(Name = nameof(DataAnnotationResources.EmailLabel), ResourceType = typeof(DataAnnotationResources))]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }

    [Required(ErrorMessageResourceName = nameof(DataAnnotationResources.PasswordRequired), ErrorMessageResourceType = typeof(DataAnnotationResources))]
    [Display(Name = nameof(DataAnnotationResources.PasswordLabel), ResourceType = typeof(DataAnnotationResources))]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Display(Name = nameof(DataAnnotationResources.RememberMeLabel), ResourceType = typeof(DataAnnotationResources))]
    public bool RememberMe { get; set; }
  }
}
