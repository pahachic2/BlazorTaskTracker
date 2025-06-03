using TaskTracker.Web.Models;

namespace TaskTracker.Web.Services;

/// <summary>
/// Реализация сервиса для управления всплывающими сообщениями
/// </summary>
public class ToastService : IToastService
{
    public event Action<ToastMessage>? OnToastAdded;
    public event Action<string>? OnToastRemoved;

    public void ShowSuccess(string title, string message, int duration = 5000)
    {
        var toast = new ToastMessage
        {
            Title = title,
            Message = message,
            Type = ToastType.Success,
            Duration = duration
        };
        ShowToast(toast);
    }

    public void ShowWarning(string title, string message, int duration = 7000)
    {
        var toast = new ToastMessage
        {
            Title = title,
            Message = message,
            Type = ToastType.Warning,
            Duration = duration
        };
        ShowToast(toast);
    }

    public void ShowError(string title, string message, int duration = 10000)
    {
        var toast = new ToastMessage
        {
            Title = title,
            Message = message,
            Type = ToastType.Error,
            Duration = duration
        };
        ShowToast(toast);
    }

    public void ShowInfo(string title, string message, int duration = 5000)
    {
        var toast = new ToastMessage
        {
            Title = title,
            Message = message,
            Type = ToastType.Info,
            Duration = duration
        };
        ShowToast(toast);
    }

    public void ShowToast(ToastMessage toast)
    {
        OnToastAdded?.Invoke(toast);
    }

    public void RemoveToast(string toastId)
    {
        OnToastRemoved?.Invoke(toastId);
    }

    public void ClearAll()
    {
        // Можно добавить событие для очистки всех, если понадобится
        OnToastRemoved?.Invoke("*"); // "*" означает удалить все
    }
} 