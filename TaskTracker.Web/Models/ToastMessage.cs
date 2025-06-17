namespace TaskTracker.Web.Models;

/// <summary>
/// Модель всплывающего сообщения
/// </summary>
public class ToastMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public ToastType Type { get; set; } = ToastType.Info;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public int Duration { get; set; } = 5000; // 5 секунд по умолчанию
    public bool IsVisible { get; set; } = true;
}

/// <summary>
/// Типы всплывающих сообщений
/// </summary>
public enum ToastType
{
    Success,    // Зеленый - успешная операция
    Warning,    // Желтый - предупреждение
    Error,      // Красный - ошибка
    Info        // Синий - информация
}

// НОВЫЕ МОДЕЛИ ДЛЯ СИСТЕМЫ УВЕДОМЛЕНИЙ

public class NotificationItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public NotificationType Type { get; set; } = NotificationType.Info;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;
    public string? ActionUrl { get; set; } // URL для перехода при клике
    public string? ActionText { get; set; } // Текст кнопки действия
    public Dictionary<string, object> Data { get; set; } = new(); // Дополнительные данные
}

public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error,
    Invitation, // Приглашение в организацию
    Task,       // Уведомления о задачах
    System      // Системные уведомления
}

public class InvitationNotificationData
{
    public string InvitationId { get; set; } = "";
    public string Token { get; set; } = "";
    public string OrganizationId { get; set; } = "";
    public string OrganizationName { get; set; } = "";
    public string InvitedByName { get; set; } = "";
    public string Role { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
} 