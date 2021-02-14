using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Yotalab.PlanningPoker.BlazorServerSide.Areas.Identity
{
  public class OverrideIdentityErrorDescriber : IdentityErrorDescriber
  {
    private ILogger<OverrideIdentityErrorDescriber> logger;

    public OverrideIdentityErrorDescriber(ILogger<OverrideIdentityErrorDescriber> logger)
    {
      this.logger = logger;
    }

    public override IdentityError DuplicateEmail(string email)
    {
      var result = base.DuplicateEmail(email);
      result.Description = "Пользователь с указаной эл. почтой уже зарегистрирован";
      return result;
    }

    public override IdentityError DuplicateUserName(string userName)
    {
      var result = base.DuplicateEmail(userName);
      result.Description = "Пользователь с таким именем уже зарегистрирован";
      return result;
    }

    public override IdentityError PasswordRequiresDigit()
    {
      var result = base.PasswordRequiresDigit();
      result.Description = "Пароль должен содержать хотя бы одну цифру";
      return result;
    }

    public override IdentityError PasswordRequiresLower()
    {
      var result = base.PasswordRequiresLower();
      result.Description = "Пароль должен содержать хотя бы одну прописную букву";
      return result;
    }

    public override IdentityError PasswordRequiresNonAlphanumeric()
    {
      var result = base.PasswordRequiresNonAlphanumeric();
      result.Description = "Пароль должен содержать хотя бы один символ не из алфавита";
      return result;
    }

    public override IdentityError PasswordRequiresUpper()
    {
      var result = base.PasswordRequiresUpper();
      result.Description = "Пароль должен содержать хотя бы одну заглавную букву";
      return result;
    }

    public override IdentityError PasswordTooShort(int length)
    {
      var result = base.PasswordTooShort(length);
      result.Description = "Пароль слишком короткий";
      return result;
    }
  }
}
