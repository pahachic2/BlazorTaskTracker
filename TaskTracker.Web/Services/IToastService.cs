using TaskTracker.Web.Models;

namespace TaskTracker.Web.Services;

/// <summary>
/// Интерфейс сервиса для управления всплывающими сообщениями
/// </summary>
public interface IToastService
{
    /// <summary>
    /// Событие при добавлении нового toast
    /// </summary>
    event Action<ToastMessage>? OnToastAdded;
    
    /// <summary>
    /// Событие при удалении toast
    /// </summary>
    event Action<string>? OnToastRemoved;

    /// <summary>
    /// Показать сообщение об успехе
    /// </summary>
    void ShowSuccess(string title, string message, int duration = 5000);
    
    /// <summary>
    /// Показать предупреждение
    /// </summary>
    void ShowWarning(string title, string message, int duration = 7000);
    
    /// <summary>
    /// Показать ошибку
    /// </summary>
    void ShowError(string title, string message, int duration = 10000);
    
    /// <summary>
    /// Показать информацию
    /// </summary>
    void ShowInfo(string title, string message, int duration = 5000);
    
    /// <summary>
    /// Показать кастомное сообщение
    /// </summary>
    void ShowToast(ToastMessage toast);
    
    /// <summary>
    /// Удалить сообщение по ID
    /// </summary>
    void RemoveToast(string toastId);
    
    /// <summary>
    /// Очистить все сообщения
    /// </summary>
    void ClearAll();
} 