using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Logging;

namespace Yotalab.PlanningPoker.BlazorServerSide.Services.Mailing
{
  public class DebugEmailSender : IEmailSender
  {
    private readonly ILogger<DebugEmailSender> logger;

    public DebugEmailSender(ILogger<DebugEmailSender> logger)
    {
      this.logger = logger;
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
      this.logger.LogDebug("Send email to: {ToEmail}, subject: {Subject}, body: {Body}",
        email, subject, HttpUtility.HtmlDecode(htmlMessage));
      return Task.CompletedTask;
    }
  }
}
