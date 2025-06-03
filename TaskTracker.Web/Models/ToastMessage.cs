namespace TaskTracker.Web.Models;

/// <summary>
/// Модель всплывающего сообщения
/// </summary>
public class ToastMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
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