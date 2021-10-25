using System.ComponentModel.DataAnnotations;

namespace Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity.Data
{
  public class LoginInputModel
  {
    [Required(ErrorMessage = "The Email field is required")]
    [EmailAddress(ErrorMessage = "The {0} field is not a valid e-mail address")]
    [Display(Name = "Email")]
    public string Email { get; set; }

    [Required(ErrorMessage = "The Password field is required")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; }

    [Display(Name = "Remember Me")]
    public bool RememberMe { get; set; }
  }
}
