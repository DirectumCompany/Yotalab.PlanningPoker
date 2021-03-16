using System.Threading.Tasks;

namespace Yotalab.PlanningPoker.BlazorServerSide.Services.Mailing
{
  public interface IEmailSender
  {
    Task SendEmailAsync(string email, string subject, string htmlMessage);
  }
}
