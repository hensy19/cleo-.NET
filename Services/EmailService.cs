using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace cleo.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        try
        {
            var smtpServer = _config["EmailSettings:SmtpServer"];
            var smtpPortString = _config["EmailSettings:SmtpPort"];
            var senderEmail = _config["EmailSettings:SenderEmail"];
            var senderPassword = _config["EmailSettings:SenderPassword"];
            var senderName = _config["EmailSettings:SenderName"];

            if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderPassword))
            {
                _logger.LogError("Email configuration is missing or incomplete in appsettings.json.");
                throw new InvalidOperationException("Email configuration is incomplete.");
            }

            int smtpPort = int.TryParse(smtpPortString, out var port) ? port : 587;

            _logger.LogInformation("Attempting to send email to {Email} via {SmtpServer}:{SmtpPort}", email, smtpServer, smtpPort);

            using var client = new SmtpClient(smtpServer, smtpPort)
            {
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = true,
                Timeout = 10000 // 10 seconds timeout
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName ?? "Cleo Support"),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };
            mailMessage.To.Add(email);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent successfully to {Email}.", email);
        }
        catch (SmtpException smtpEx)
        {
            _logger.LogError(smtpEx, "SMTP error occurred while sending email to {Email}. Status Code: {StatusCode}", email, smtpEx.StatusCode);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "General error occurred while sending email to {Email}.", email);
            throw;
        }
    }
}
