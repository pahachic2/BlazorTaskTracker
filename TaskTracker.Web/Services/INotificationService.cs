using TaskTracker.Web.Models;
using TaskTracker.Models.DTOs;

namespace TaskTracker.Web.Services;

public interface INotificationService
{
    // Получить все уведомления
    Task<List<NotificationItem>> GetNotificationsAsync();
    
    // Получить количество непрочитанных уведомлений
    Task<int> GetUnreadCountAsync();
    
    // Пометить уведомление как прочитанное
    Task MarkAsReadAsync(string notificationId);
    
    // Пометить все уведомления как прочитанные
    Task MarkAllAsReadAsync();
    
    // Удалить уведомление
    Task DeleteNotificationAsync(string notificationId);
    
    // Добавить уведомление о приглашении
    Task AddInvitationNotificationAsync(InvitationResponse invitation);
    
    // Принять приглашение из уведомления
    Task<bool> AcceptInvitationFromNotificationAsync(string notificationId);
    
    // Отклонить приглашение из уведомления
    Task<bool> DeclineInvitationFromNotificationAsync(string notificationId);
    
    // Событие для обновления UI
    event Action? NotificationsChanged;
    
    // ТЕСТОВЫЕ МЕТОДЫ (можно удалить после тестирования)
    Task AddTestNotificationsAsync();
    Task AddTestInvitationNotificationAsync(NotificationItem invitation);
    
    // Новый метод для обновления уведомлений
    Task RefreshNotificationsAsync();
} 