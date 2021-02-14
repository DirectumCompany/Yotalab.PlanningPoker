using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Yotalab.PlanningPoker.BlazorServerSide.Services.Mailing
{
  public class SmtpEmailSender : IEmailSender
  {
    protected readonly string host;
    protected readonly int port;
    protected readonly bool enableSSL;
    protected readonly string userName;
    protected readonly string password;

    public SmtpEmailSender(string host, int port, bool enableSSL, string userName, string password)
    {
      this.host = host;
      this.port = port;
      this.enableSSL = enableSSL;
      this.userName = userName;
      this.password = password;
    }

    #region IEmailSender

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
      using var client = this.CreateClient();
      using var message = new MailMessage(this.userName, email, subject, htmlMessage) { IsBodyHtml = true };
      return client.SendMailAsync(message);
    }

    #endregion

    #region Методы

    private SmtpClient CreateClient()
    {
      return new SmtpClient(this.host, this.port)
      {
        Credentials = new NetworkCredential(this.userName, this.password),
        EnableSsl = this.enableSSL
      };
    }

    #endregion
  }
}
