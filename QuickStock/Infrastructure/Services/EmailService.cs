using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
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
            using var message = new MailMessage();
            message.From = new MailAddress(_settings.SmtpUser, "QuickStock");
            message.To.Add(new MailAddress(to));
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = isHtml;

            using var client = new SmtpClient(_settings.SmtpServer, _settings.SmtpPort);
            client.Credentials = new NetworkCredential(_settings.SmtpUser, _settings.SmtpPass);
            client.EnableSsl = true; // SmtpClient expects EnableSsl for port 587
            await client.SendMailAsync(message);
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
