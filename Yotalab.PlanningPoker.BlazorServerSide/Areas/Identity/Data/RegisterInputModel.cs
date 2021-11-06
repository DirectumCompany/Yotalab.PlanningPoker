using System.ComponentModel.DataAnnotations;
using Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Resources;

namespace Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Data
{
  public class RegisterInputModel
  {
    [Required(ErrorMessageResourceName = nameof(DataAnnotationResources.EmailRequired), ErrorMessageResourceType = typeof(DataAnnotationResources))]
    [EmailAddress(ErrorMessageResourceName = nameof(DataAnnotationResources.EmailInvalid), ErrorMessageResourceType = typeof(DataAnnotationResources))]
    [Display(Name = nameof(DataAnnotationResources.EmailLabel), ResourceType = typeof(DataAnnotationResources))]
    public string Email { get; set; }

    [Required(ErrorMessageResourceName = nameof(DataAnnotationResources.PasswordRequired), ErrorMessageResourceType = typeof(DataAnnotationResources))]
    [StringLength(100, MinimumLength = 6, ErrorMessageResourceName = nameof(DataAnnotationResources.PasswordLengthInvalid), ErrorMessageResourceType = typeof(DataAnnotationResources))]
    [Display(Name = nameof(DataAnnotationResources.PasswordLabel), ResourceType = typeof(DataAnnotationResources))]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = nameof(DataAnnotationResources.ConfirmPasswordLabel), ResourceType = typeof(DataAnnotationResources))]
    [Compare("Password", ErrorMessageResourceName = nameof(DataAnnotationResources.PasswordAndConfirmNotMatch), ErrorMessageResourceType = typeof(DataAnnotationResources))]
    public string ConfirmPassword { get; set; }
  }
}
