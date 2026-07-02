using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using SV.Common.Abstractions;

namespace SV.Service.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string bodyHtml)
        {
            var host = _configuration["SmtpSettings:Host"] ?? "smtp.gmail.com";
            var portStr = _configuration["SmtpSettings:Port"] ?? "587";
            var enableSslStr = _configuration["SmtpSettings:EnableSsl"] ?? "true";
            var senderName = _configuration["SmtpSettings:SenderName"] ?? "StreamVault Support";
            var senderEmail = _configuration["SmtpSettings:SenderEmail"] ?? "no-reply@streamvault.com";
            var username = _configuration["SmtpSettings:Username"];
            var password = _configuration["SmtpSettings:Password"];

            int.TryParse(portStr, out var port);
            bool.TryParse(enableSslStr, out var enableSsl);

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = bodyHtml };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();

            // Support both SSL/TLS direct and STARTTLS depending on the port
            var secureSocketOption = MailKit.Security.SecureSocketOptions.Auto;
            if (port == 465)
            {
                secureSocketOption = MailKit.Security.SecureSocketOptions.SslOnConnect;
            }
            else if (port == 587)
            {
                secureSocketOption = MailKit.Security.SecureSocketOptions.StartTls;
            }
            else if (!enableSsl)
            {
                secureSocketOption = MailKit.Security.SecureSocketOptions.None;
            }

            await client.ConnectAsync(host, port, secureSocketOption);

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                await client.AuthenticateAsync(username, password);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
