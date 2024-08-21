using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class EmailService
{
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _fromAddress;
    private readonly string _accessToken;
    private readonly ILogger<EmailService> _logger;

    public EmailService(string smtpServer, int smtpPort, string fromAddress, string accessToken, ILogger<EmailService> logger)
    {
        _smtpServer = smtpServer;
        _smtpPort = smtpPort;
        _fromAddress = fromAddress;
        _accessToken = accessToken;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toAddress, string subject, string body)
    {
        _logger.LogInformation("Starting to send email.");
        _logger.LogInformation($"SMTP Server: {_smtpServer}, Port: {_smtpPort}");
        _logger.LogInformation($"From: {_fromAddress}, To: {toAddress}, Subject: {subject}");

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(string.Empty, _fromAddress));
            message.To.Add(new MailboxAddress(string.Empty, toAddress));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_fromAddress, _accessToken);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }

            _logger.LogInformation("Email sent successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while sending email.");
            throw;
        }
    }
}
