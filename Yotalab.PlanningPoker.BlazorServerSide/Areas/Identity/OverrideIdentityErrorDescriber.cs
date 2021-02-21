using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Yotalab.PlanningPoker.BlazorServerSide.Resources;

namespace Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity
{
  public class OverrideIdentityErrorDescriber : IdentityErrorDescriber
  {
    private IStringLocalizer<IdentityResource> localizer;

    public OverrideIdentityErrorDescriber(IStringLocalizer<IdentityResource> localizer)
    {
      this.localizer = localizer;
    }

    public override IdentityError DuplicateEmail(string email)
    {
      return this.TryLocalize(base.DuplicateEmail(email), email);
    }

    public override IdentityError DuplicateUserName(string userName)
    {
      // Сейчас имя пользователя совпадает с эл. почтой.
      return this.TryLocalize(base.DuplicateEmail(userName), userName);
    }

    public override IdentityError PasswordRequiresDigit()
    {
      return this.TryLocalize(base.PasswordRequiresDigit());
    }

    public override IdentityError PasswordRequiresLower()
    {
      return this.TryLocalize(base.PasswordRequiresLower());
    }

    public override IdentityError PasswordRequiresNonAlphanumeric()
    {
      return this.TryLocalize(base.PasswordRequiresNonAlphanumeric());
    }

    public override IdentityError PasswordRequiresUpper()
    {
      return this.TryLocalize(base.PasswordRequiresUpper());
    }

    public override IdentityError PasswordTooShort(int length)
    {
      return this.TryLocalize(base.PasswordTooShort(length), length);
    }

    private IdentityError TryLocalize(IdentityError identityError)
    {
      return this.TryLocalize(identityError, new object[0]);
    }

    private IdentityError TryLocalize(IdentityError identityError, params object[] arguments)
    {
      var localizedString = this.localizer.GetString(identityError.Code, arguments);
      if (!localizedString.ResourceNotFound)
        identityError.Description = localizedString.Value;

      return identityError;
    }
  }
}
