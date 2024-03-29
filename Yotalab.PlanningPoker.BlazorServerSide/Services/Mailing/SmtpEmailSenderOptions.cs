﻿namespace Yotalab.PlanningPoker.BlazorServerSide.Services.Mailing
{
  public class SmtpEmailSenderOptions
  {
    public SmtpEmailSenderOptions(string host, int port, bool enableSSL, string userName, string password, string from)
    {
      this.Host = host;
      this.Port = port;
      this.EnableSSL = enableSSL;
      this.UserName = userName;
      this.Password = password;
      this.From = from;
    }

    public string Host { get; }

    public int Port { get; }

    public bool EnableSSL { get; }

    public string UserName { get; }

    public string Password { get; }

    public string From { get; }
  }
}
