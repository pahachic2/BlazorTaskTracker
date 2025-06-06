using TaskTracker.Models;

namespace TaskTracker.Api.Services;

/// <summary>
/// Интерфейс для отправки email уведомлений
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Отправить приглашение в организацию
    /// </summary>
    Task<bool> SendInvitationEmailAsync(string toEmail, string organizationName, string invitedByName, string invitationToken, OrganizationRole role);

    /// <summary>
    /// Отправить приветственное письмо после принятия приглашения
    /// </summary>
    Task<bool> SendWelcomeEmailAsync(string toEmail, string organizationName);

    /// <summary>
    /// Отправить уведомление об отзыве приглашения
    /// </summary>
    Task<bool> SendInvitationRevokedEmailAsync(string toEmail, string organizationName, string revokedByName);

    /// <summary>
    /// Проверить настройки email (для диагностики)
    /// </summary>
    Task<bool> TestEmailConfigurationAsync();
} 