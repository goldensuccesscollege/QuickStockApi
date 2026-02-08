using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using QuickStock.Infrastructure.Config;
using System.Threading.Tasks;

namespace QuickStock.Infrastructure.Services
{
    public class EmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        // Generic email sender
        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("QuickStock", _settings.SmtpUser));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = isHtml ? body : null,
                TextBody = isHtml ? null : body
            };

            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.SmtpServer, _settings.SmtpPort, false);
            await client.AuthenticateAsync(_settings.SmtpUser, _settings.SmtpPass);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        // Password reset email
        public async Task SendResetPasswordEmailAsync(string to, string resetLink)
        {
            string subject = "QuickStock Password Reset Request";

            string body = $@"
                    Hello,

                    We received a request to reset your QuickStock password.

                    Please click the link below to reset your password (valid for 1 hour, one-time use):

                    {resetLink}

                    If you did not request this, you can ignore this email.

                    QuickStock Team
                    ";

            await SendEmailAsync(to, subject, body, isHtml: false);
        }
    }
}
