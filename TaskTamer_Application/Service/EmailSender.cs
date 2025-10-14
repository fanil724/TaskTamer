using NLog;
using Microsoft.AspNetCore.Identity.UI.Services;
namespace TaskTamer_Application.Service;

public class EmailSender : IEmailSender
{
    private readonly Logger  _logger=LogManager.GetCurrentClassLogger();

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        
       /* var emailsDir = Path.Combine(Directory.GetCurrentDirectory(), "Emails");
        Directory.CreateDirectory(emailsDir);
        var filePath = Path.Combine(emailsDir, $"{DateTime.Now:yyyyMMdd-HHmmss}-{email}.html");

        await File.WriteAllTextAsync(filePath, htmlMessage);*/
       
        _logger.Info($"Email saved");
    }
}