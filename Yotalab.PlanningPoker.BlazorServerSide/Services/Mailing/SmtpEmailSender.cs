using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Yotalab.PlanningPoker.BlazorServerSide.Services.Mailing
{
  public class SmtpEmailSender : IEmailSender
  {
    private readonly SmtpEmailSenderOptions options;
    private readonly ILogger<SmtpEmailSender> logger;

    public SmtpEmailSender(SmtpEmailSenderOptions options, ILogger<SmtpEmailSender> logger)
    {
      this.options = options;
      this.logger = logger;
    }

    #region IEmailSender

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
      try
      {
        using var client = this.CreateClient();
        using var message = new MailMessage(this.options.UserName, email, subject, htmlMessage) { IsBodyHtml = true };
        if (!string.IsNullOrWhiteSpace(this.options.From))
          message.From = new MailAddress(this.options.From);
        await client.SendMailAsync(message);
      }
      catch (Exception ex)
      {
        this.logger.LogError(ex, "SendEmailAsync failed: From:{From}, To:{To}, Subject:{Subject}", this.options.UserName, email, subject);
        throw;
      }
    }

    #endregion

    #region Методы

    private SmtpClient CreateClient()
    {
      return new SmtpClient(this.options.Host, this.options.Port)
      {
        Credentials = new NetworkCredential(this.options.UserName, this.options.Password),
        EnableSsl = this.options.EnableSSL
      };
    }

    #endregion
  }
}
