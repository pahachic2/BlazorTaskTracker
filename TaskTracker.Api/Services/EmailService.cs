using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using TaskTracker.Api.Configuration;
using TaskTracker.Models;

namespace TaskTracker.Api.Services;

/// <summary>
/// Сервис для отправки email уведомлений
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task<bool> SendInvitationEmailAsync(string toEmail, string organizationName, string invitedByName, string invitationToken, OrganizationRole role)
    {
        Console.WriteLine($"📧 EMAIL: Отправка приглашения в организацию {organizationName} на email {toEmail}");

        if (!_emailSettings.EnableEmailSending)
        {
            Console.WriteLine($"⚠️ EMAIL: Отправка email отключена в настройках");
            return true; // Возвращаем true для разработки
        }

        try
        {
            var subject = $"Приглашение в организацию {organizationName} в TaskTracker";
            var body = GenerateInvitationEmailHtml(organizationName, invitedByName, invitationToken, role);

            return await SendEmailAsync(toEmail, subject, body);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ EMAIL: Ошибка отправки приглашения: {ex.Message}");
            _logger.LogError(ex, "Ошибка отправки email приглашения для {Email}", toEmail);
            return false;
        }
    }

    public async Task<bool> SendWelcomeEmailAsync(string toEmail, string organizationName)
    {
        Console.WriteLine($"📧 EMAIL: Отправка приветственного письма для организации {organizationName} на email {toEmail}");

        if (!_emailSettings.EnableEmailSending)
        {
            Console.WriteLine($"⚠️ EMAIL: Отправка email отключена в настройках");
            return true;
        }

        try
        {
            var subject = $"Добро пожаловать в организацию {organizationName}!";
            var body = GenerateWelcomeEmailHtml(organizationName);

            return await SendEmailAsync(toEmail, subject, body);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ EMAIL: Ошибка отправки приветственного письма: {ex.Message}");
            _logger.LogError(ex, "Ошибка отправки приветственного email для {Email}", toEmail);
            return false;
        }
    }

    public async Task<bool> SendInvitationRevokedEmailAsync(string toEmail, string organizationName, string revokedByName)
    {
        Console.WriteLine($"📧 EMAIL: Отправка уведомления об отзыве приглашения для организации {organizationName} на email {toEmail}");

        if (!_emailSettings.EnableEmailSending)
        {
            Console.WriteLine($"⚠️ EMAIL: Отправка email отключена в настройках");
            return true;
        }

        try
        {
            var subject = $"Приглашение в организацию {organizationName} отозвано";
            var body = GenerateInvitationRevokedEmailHtml(organizationName, revokedByName);

            return await SendEmailAsync(toEmail, subject, body);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ EMAIL: Ошибка отправки уведомления об отзыве: {ex.Message}");
            _logger.LogError(ex, "Ошибка отправки email об отзыве приглашения для {Email}", toEmail);
            return false;
        }
    }

    public async Task<bool> TestEmailConfigurationAsync()
    {
        Console.WriteLine($"🔧 EMAIL: Тестирование конфигурации email");

        if (!_emailSettings.EnableEmailSending)
        {
            Console.WriteLine($"⚠️ EMAIL: Отправка email отключена в настройках");
            return true;
        }

        try
        {
            using var smtpClient = CreateSmtpClient();
            await smtpClient.SendMailAsync(
                _emailSettings.FromEmail,
                _emailSettings.FromEmail,
                "Тест конфигурации TaskTracker",
                "Конфигурация email работает корректно!"
            );

            Console.WriteLine($"✅ EMAIL: Тест конфигурации прошел успешно");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ EMAIL: Ошибка тестирования конфигурации: {ex.Message}");
            _logger.LogError(ex, "Ошибка тестирования email конфигурации");
            return false;
        }
    }

    private async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        try
        {
            using var smtpClient = CreateSmtpClient();
            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
            Console.WriteLine($"✅ EMAIL: Письмо успешно отправлено на {toEmail}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ EMAIL: Ошибка отправки письма на {toEmail}: {ex.Message}");
            _logger.LogError(ex, "Ошибка отправки email на {Email}", toEmail);
            return false;
        }
    }

    private SmtpClient CreateSmtpClient()
    {
        var smtpClient = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort)
        {
            EnableSsl = _emailSettings.EnableSsl,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword)
        };

        return smtpClient;
    }

    private string GenerateInvitationEmailHtml(string organizationName, string invitedByName, string invitationToken, OrganizationRole role)
    {
        var roleText = role switch
        {
            OrganizationRole.Owner => "владельца",
            OrganizationRole.Admin => "администратора",
            OrganizationRole.Member => "участника",
            _ => "участника"
        };

        var acceptUrl = $"{_emailSettings.BaseUrl}/invite/accept?token={invitationToken}";

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Приглашение в организацию {organizationName}</title>
    <style>
        body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: white; padding: 30px; border: 1px solid #e1e5e9; }}
        .footer {{ background: #f8f9fa; padding: 20px; text-align: center; border-radius: 0 0 10px 10px; border: 1px solid #e1e5e9; border-top: none; }}
        .btn {{ display: inline-block; padding: 12px 30px; background: #28a745; color: white; text-decoration: none; border-radius: 5px; font-weight: bold; margin: 20px 0; }}
        .btn:hover {{ background: #218838; }}
        .organization {{ background: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>📋 TaskTracker</h1>
            <h2>Приглашение в организацию</h2>
        </div>
        <div class='content'>
            <p>Здравствуйте!</p>
            
            <p><strong>{invitedByName}</strong> приглашает вас присоединиться к организации в системе управления задачами TaskTracker.</p>
            
            <div class='organization'>
                <h3>🏢 {organizationName}</h3>
                <p><strong>Роль:</strong> {roleText}</p>
            </div>
            
            <p>TaskTracker поможет вашей команде эффективно управлять проектами и задачами с помощью канбан досок.</p>
            
            <div style='text-align: center;'>
                <a href='{acceptUrl}' class='btn'>Принять приглашение</a>
            </div>
            
            <p><small>Если вы не можете нажать на кнопку, скопируйте эту ссылку в браузер:<br>
            <a href='{acceptUrl}'>{acceptUrl}</a></small></p>
            
            <p><small>Приглашение действительно в течение 7 дней. Если вы не хотите присоединяться к этой организации, просто проигнорируйте это письмо.</small></p>
        </div>
        <div class='footer'>
            <p><small>© 2024 TaskTracker. Система управления задачами.</small></p>
        </div>
    </div>
</body>
</html>";
    }

    private string GenerateWelcomeEmailHtml(string organizationName)
    {
        var dashboardUrl = $"{_emailSettings.BaseUrl}/projects";

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Добро пожаловать в {organizationName}!</title>
    <style>
        body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #28a745 0%, #20c997 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: white; padding: 30px; border: 1px solid #e1e5e9; }}
        .footer {{ background: #f8f9fa; padding: 20px; text-align: center; border-radius: 0 0 10px 10px; border: 1px solid #e1e5e9; border-top: none; }}
        .btn {{ display: inline-block; padding: 12px 30px; background: #007bff; color: white; text-decoration: none; border-radius: 5px; font-weight: bold; margin: 20px 0; }}
        .features {{ display: grid; grid-template-columns: 1fr 1fr; gap: 15px; margin: 20px 0; }}
        .feature {{ background: #f8f9fa; padding: 15px; border-radius: 5px; text-align: center; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 Добро пожаловать!</h1>
            <h2>Вы успешно присоединились к организации</h2>
        </div>
        <div class='content'>
            <p>Поздравляем! Вы успешно присоединились к организации <strong>{organizationName}</strong> в TaskTracker.</p>
            
            <h3>🚀 Что теперь можно делать:</h3>
            <div class='features'>
                <div class='feature'>
                    <h4>📋 Канбан доски</h4>
                    <p>Управляйте задачами с помощью удобных досок</p>
                </div>
                <div class='feature'>
                    <h4>👥 Командная работа</h4>
                    <p>Назначайте задачи коллегам из организации</p>
                </div>
                <div class='feature'>
                    <h4>📊 Проекты</h4>
                    <p>Создавайте и организуйте проекты команды</p>
                </div>
                <div class='feature'>
                    <h4>🏷️ Теги и приоритеты</h4>
                    <p>Организуйте задачи по категориям</p>
                </div>
            </div>
            
            <div style='text-align: center;'>
                <a href='{dashboardUrl}' class='btn'>Перейти к проектам</a>
            </div>
        </div>
        <div class='footer'>
            <p><small>© 2024 TaskTracker. Система управления задачами.</small></p>
        </div>
    </div>
</body>
</html>";
    }

    private string GenerateInvitationRevokedEmailHtml(string organizationName, string revokedByName)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Приглашение отозвано</title>
    <style>
        body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #dc3545 0%, #c82333 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: white; padding: 30px; border: 1px solid #e1e5e9; }}
        .footer {{ background: #f8f9fa; padding: 20px; text-align: center; border-radius: 0 0 10px 10px; border: 1px solid #e1e5e9; border-top: none; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>📋 TaskTracker</h1>
            <h2>Приглашение отозвано</h2>
        </div>
        <div class='content'>
            <p>Уведомляем вас о том, что приглашение в организацию <strong>{organizationName}</strong> было отозвано администратором <strong>{revokedByName}</strong>.</p>
            
            <p>Если у вас есть вопросы по этому поводу, обратитесь к администратору организации.</p>
            
            <p>Спасибо за интерес к нашей платформе!</p>
        </div>
        <div class='footer'>
            <p><small>© 2024 TaskTracker. Система управления задачами.</small></p>
        </div>
    </div>
</body>
</html>";
    }
} 