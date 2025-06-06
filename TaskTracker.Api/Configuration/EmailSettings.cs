namespace TaskTracker.Api.Configuration;

/// <summary>
/// Настройки для отправки email
/// </summary>
public class EmailSettings
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "TaskTracker";
    public string BaseUrl { get; set; } = "https://localhost:5173"; // URL frontend приложения
    public bool EnableEmailSending { get; set; } = true; // Возможность отключить отправку в разработке
} 