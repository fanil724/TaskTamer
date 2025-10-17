using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using NLog;
using System.Net;
using System.Net.Mail;
namespace TaskTamer_Application.Service;

public class EmailSender : IEmailSender
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private readonly SmtpClient _smtpClient;
   

    public EmailSender(IConfiguration configuration)
    {
       
        _smtpClient = new SmtpClient
        {
            Host = configuration["Email:SmtpHost"],
            Port = int.Parse(configuration["Email:SmtpPort"]),
            Credentials = new NetworkCredential(
                configuration["Email:Username"],
                configuration["Email:Password"]),
            EnableSsl = bool.Parse(configuration["Email:EnableSsl"] ?? "true")
        };
    }


    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        try
        {
            var message = BuildNotificationTemplate(htmlMessage);
            var mailMessage = new MailMessage
            {
                From = new MailAddress("noreply@TaskTamer.com"),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };
            mailMessage.To.Add(email);

            await _smtpClient.SendMailAsync(mailMessage);

            _logger.Info($"Email успешно отправлен на {email}. Тема: {subject}");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Ошибка при отправке email на {email}");
            
        }
    }

    public async Task SendNotificationAsync(string email,  string message)
    {
        try
        {
            var htmlMessage = BuildNotificationTemplate(message);

            await SendEmailAsync(email, "Уведомление от системы", htmlMessage);

            _logger.Info($"Уведомление  отправлено на {email}");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Ошибка при отправке уведомления  на {email}");
           
        }
    }

    private string BuildNotificationTemplate(string message)
    {
        return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background-color: #f8f9fa; padding: 20px; text-align: center; }}
                    .content {{ padding: 20px; background-color: #fff; }}
                    .footer {{ padding: 20px; background-color: #f8f9fa; text-align: center; font-size: 12px; color: #666; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h2>Уведомление</h2>
                    </div>
                    <div class='content'>
                        <p>{message}</p>                       
                    </div>
                    <div class='footer'>
                       <p>Это автоматическое сообщение, пожалуйста, не отвечайте на него.<br>
                             Если у вас есть вопросы, обратитесь в службу поддержки.</p>
                    </div>
                </div>
            </body>
            </html>";
    }

}