// Services/EmailSender.cs
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using System;
using System.Threading.Tasks;

namespace CaterManagementSystem.Services
{
    public class EmailSender : IEmailSender // IEmailSender interfeysini implement edir
    {
        private readonly MailSettings _mailSettings;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IOptions<MailSettings> mailSettings, ILogger<EmailSender> logger)
        {
            _mailSettings = mailSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            if (string.IsNullOrEmpty(_mailSettings.SmtpServer) ||
                string.IsNullOrEmpty(_mailSettings.SmtpUsername) ||
                string.IsNullOrEmpty(_mailSettings.SmtpPassword) ||
                string.IsNullOrEmpty(_mailSettings.FromAddress))
            {
                _logger.LogError("Mail settings are not configured properly. Email not sent to {ToEmail}", toEmail);
                // Production-da daha detallı bir xəta mesajı və ya xüsusi bir exception ata bilərsiniz
                throw new InvalidOperationException("Email settings are not properly configured in appsettings.json.");
            }

            try
            {
                var emailMessage = new MimeMessage();

                emailMessage.From.Add(new MailboxAddress(_mailSettings.FromName ?? "Cater Management System", _mailSettings.FromAddress));
                emailMessage.To.Add(MailboxAddress.Parse(toEmail));
                emailMessage.Subject = subject;

                var builder = new BodyBuilder { HtmlBody = htmlMessage };
                emailMessage.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();

                SecureSocketOptions socketOptions;
                if (_mailSettings.UseSsl) // Port 465 üçün adətən true olur
                {
                    socketOptions = SecureSocketOptions.SslOnConnect;
                }
                else // Port 587 üçün adətən false (StartTls istifadə olunur)
                {
                    // Port 25 üçün də false (StartTlsOptional və ya None)
                    socketOptions = SecureSocketOptions.StartTlsWhenAvailable; // Və ya StartTls
                }
                // Sizin konfiqurasiyada SmtpPort=587 və UseSsl=false, bu StartTls deməkdir.
                if (_mailSettings.SmtpPort == 587 && !_mailSettings.UseSsl)
                {
                    socketOptions = SecureSocketOptions.StartTls;
                }


                await smtp.ConnectAsync(_mailSettings.SmtpServer, _mailSettings.SmtpPort, socketOptions);
                await smtp.AuthenticateAsync(_mailSettings.SmtpUsername, _mailSettings.SmtpPassword);
                await smtp.SendAsync(emailMessage);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation("Email sent successfully to {ToEmail} with subject {Subject}", toEmail, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending email to {ToEmail} with subject {Subject}. Error: {ErrorMessage}", toEmail, subject, ex.Message);
                // Burada xətanı yenidən ata bilərsiniz və ya istifadəçiyə ümumi bir mesaj göstərə bilərsiniz.
                throw; // Controller səviyyəsində tutulub istifadəçiyə mesaj verilə bilər
            }
        }
    }
}