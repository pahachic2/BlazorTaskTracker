using System.Text.Json;
using TaskTracker.Web.Models;
using TaskTracker.Models.DTOs;
using TaskTracker.Models;

namespace TaskTracker.Web.Services;

public class NotificationService : INotificationService
{
    private readonly ILocalStorageService _localStorage;
    private readonly IApiService _apiService;
    private readonly OrganizationInvitationService _invitationService;
    private readonly IToastService _toastService;
    private readonly string _storageKey = "user_notifications";
    
    private List<NotificationItem> _notifications = new();
    
    public event Action? NotificationsChanged;

    public NotificationService(
        ILocalStorageService localStorage,
        IApiService apiService,
        OrganizationInvitationService invitationService,
        IToastService toastService)
    {
        _localStorage = localStorage;
        _apiService = apiService;
        _invitationService = invitationService;
        _toastService = toastService;
    }

    public Task<List<NotificationItem>> GetNotificationsAsync()
    {
        Console.WriteLine("🔔 NOTIFICATIONS: Запрос на получение уведомлений");
        Console.WriteLine($"🔔 NOTIFICATIONS: Текущее количество уведомлений в памяти: {_notifications.Count}");
        
        // Просто возвращаем текущий список, отсортированный по дате
        var sortedNotifications = _notifications.OrderByDescending(n => n.CreatedAt).ToList();
        Console.WriteLine($"🔔 NOTIFICATIONS: Возвращаем {sortedNotifications.Count} уведомлений");
        return Task.FromResult(sortedNotifications);
    }

    public async Task RefreshNotificationsAsync()
    {
        Console.WriteLine("🔔 NOTIFICATIONS: Принудительная перезагрузка уведомлений");
        
        try
        {
            Console.WriteLine("🔔 NOTIFICATIONS: Загружаем уведомления из LocalStorage");
            await LoadNotificationsFromStorageAsync();
            Console.WriteLine($"🔔 NOTIFICATIONS: Загружено из storage: {_notifications.Count} уведомлений");

            Console.WriteLine("🔔 NOTIFICATIONS: Загружаем приглашения с сервера");
            await LoadInvitationNotificationsAsync();
            Console.WriteLine($"🔔 NOTIFICATIONS: После загрузки приглашений: {_notifications.Count} уведомлений");

            foreach (var notification in _notifications)
            {
                Console.WriteLine($"🔔 NOTIFICATIONS: Уведомление: ID={notification.Id}, Type={notification.Type}, Title={notification.Title}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ NOTIFICATIONS: Ошибка загрузки уведомлений: {ex.Message}");
            _notifications = new List<NotificationItem>();
        }
        
        // Сортируем по дате создания (новые сначала)
        _notifications = _notifications.OrderByDescending(n => n.CreatedAt).ToList();
        
        Console.WriteLine($"🔔 NOTIFICATIONS: Всего уведомлений: {_notifications.Count}");
        NotificationsChanged?.Invoke();
    }

    public async Task MarkAsReadAsync(string notificationId)
    {
        var notification = _notifications.FirstOrDefault(n => n.Id == notificationId);
        if (notification != null)
        {
            notification.IsRead = true;
            await SaveNotificationsToStorageAsync();
            NotificationsChanged?.Invoke();
        }
    }

    public async Task MarkAllAsReadAsync()
    {
        foreach (var notification in _notifications)
        {
            notification.IsRead = true;
        }
        await SaveNotificationsToStorageAsync();
        NotificationsChanged?.Invoke();
    }

    public async Task DeleteNotificationAsync(string notificationId)
    {
        _notifications.RemoveAll(n => n.Id == notificationId);
        await SaveNotificationsToStorageAsync();
        NotificationsChanged?.Invoke();
    }

    public async Task<int> GetUnreadCountAsync()
    {
        await LoadNotificationsFromStorageAsync();
        await LoadInvitationNotificationsAsync();
        return _notifications.Count(n => !n.IsRead);
    }

    public async Task AddInvitationNotificationAsync(InvitationResponse invitation)
    {
        // Проверяем, нет ли уже такого уведомления
        var existingNotification = _notifications.FirstOrDefault(n => 
            n.Type == NotificationType.Invitation && 
            n.Data.ContainsKey("InvitationId") && 
            n.Data["InvitationId"].ToString() == invitation.Id);

        if (existingNotification == null)
        {
            var notification = new NotificationItem
            {
                Title = "Приглашение в организацию",
                Message = $"Вас пригласили в организацию \"{invitation.OrganizationName}\" в роли {GetRoleDisplayName(invitation.Role)}",
                Type = NotificationType.Invitation,
                CreatedAt = invitation.CreatedAt,
                IsRead = false,
                ActionText = "Принять/Отклонить",
                Data = new Dictionary<string, object>
                {
                    ["InvitationId"] = invitation.Id,
                    ["Token"] = invitation.Token,
                    ["OrganizationId"] = invitation.OrganizationId,
                    ["OrganizationName"] = invitation.OrganizationName,
                    ["InvitedByName"] = invitation.InvitedByName,
                    ["Role"] = invitation.Role.ToString(),
                    ["ExpiresAt"] = invitation.ExpiresAt
                }
            };

            _notifications.Add(notification);
            await SaveNotificationsToStorageAsync();
            NotificationsChanged?.Invoke();
        }
    }

    public async Task<bool> AcceptInvitationFromNotificationAsync(string notificationId)
    {
        Console.WriteLine($"🔔 NOTIFICATIONS: Начинаем принятие приглашения для уведомления {notificationId}");
        Console.WriteLine($"🔔 NOTIFICATIONS: Всего уведомлений в списке: {_notifications.Count}");
        
        foreach (var n in _notifications)
        {
            Console.WriteLine($"🔔 NOTIFICATIONS: ID: {n.Id}, Type: {n.Type}, HasToken: {n.Data.ContainsKey("Token")}");
        }
        
        var notification = _notifications.FirstOrDefault(n => n.Id == notificationId);
        if (notification?.Type != NotificationType.Invitation || !notification.Data.ContainsKey("Token"))
        {
            Console.WriteLine($"❌ NOTIFICATIONS: Уведомление не найдено или не является приглашением. Notification: {notification != null}, Type: {notification?.Type}, HasToken: {notification?.Data.ContainsKey("Token")}");
            return false;
        }

        try
        {
            var token = notification.Data["Token"].ToString()!;
            Console.WriteLine($"🔔 NOTIFICATIONS: Извлекли токен: {token[..10]}...");
            
            var request = new AcceptInvitationRequest { Token = token };
            Console.WriteLine($"🔔 NOTIFICATIONS: Создали запрос, вызываем _invitationService.AcceptInvitationAsync");
            
            var result = await _invitationService.AcceptInvitationAsync(request);
            Console.WriteLine($"🔔 NOTIFICATIONS: Получили результат от invitation service: Success={result?.Success}, Message={result?.Message}");

            if (result?.Success == true)
            {
                Console.WriteLine($"🔔 NOTIFICATIONS: Приглашение принято успешно, удаляем уведомление");
                // Удаляем уведомление после успешного принятия
                await DeleteNotificationAsync(notificationId);
                _toastService.ShowSuccess("Успех", "Приглашение принято! Добро пожаловать в организацию.");
                return true;
            }
            else
            {
                Console.WriteLine($"❌ NOTIFICATIONS: Принятие не удалось: {result?.Message}");
                _toastService.ShowError("Ошибка", result?.Message ?? "Не удалось принять приглашение");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ NOTIFICATIONS: Ошибка принятия приглашения: {ex.Message}");
            Console.WriteLine($"❌ NOTIFICATIONS: Stack trace: {ex.StackTrace}");
            _toastService.ShowError("Ошибка", "Произошла ошибка при принятии приглашения");
            return false;
        }
    }

    public async Task<bool> DeclineInvitationFromNotificationAsync(string notificationId)
    {
        var notification = _notifications.FirstOrDefault(n => n.Id == notificationId);
        if (notification?.Type != NotificationType.Invitation || !notification.Data.ContainsKey("Token"))
            return false;

        try
        {
            var token = notification.Data["Token"].ToString()!;
            var success = await _invitationService.DeclineInvitationAsync(token);

            if (success)
            {
                // Удаляем уведомление после успешного отклонения
                await DeleteNotificationAsync(notificationId);
                _toastService.ShowInfo("Уведомление", "Приглашение отклонено");
                return true;
            }
            else
            {
                _toastService.ShowError("Ошибка", "Не удалось отклонить приглашение");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ NOTIFICATIONS: Ошибка отклонения приглашения: {ex.Message}");
            _toastService.ShowError("Ошибка", "Произошла ошибка при отклонении приглашения");
            return false;
        }
    }

    private async Task LoadNotificationsFromStorageAsync()
    {
        try
        {
            var storedJson = await _localStorage.GetItemAsync(_storageKey);
            if (!string.IsNullOrEmpty(storedJson))
            {
                var stored = JsonSerializer.Deserialize<List<NotificationItem>>(storedJson);
                if (stored != null)
                {
                    // Фильтруем просроченные уведомления (старше 30 дней)
                    var cutoffDate = DateTime.UtcNow.AddDays(-30);
                    _notifications = stored.Where(n => n.CreatedAt > cutoffDate).ToList();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ NOTIFICATIONS: Ошибка загрузки из localStorage: {ex.Message}");
            _notifications = new List<NotificationItem>();
        }
    }

    private async Task SaveNotificationsToStorageAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_notifications);
            await _localStorage.SetItemAsync(_storageKey, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ NOTIFICATIONS: Ошибка сохранения в localStorage: {ex.Message}");
        }
    }

    private async Task LoadInvitationNotificationsAsync()
    {
        try
        {
            var invitations = await _apiService.GetUserInvitationsAsync();
            
            // Удаляем старые уведомления о приглашениях
            _notifications.RemoveAll(n => n.Type == NotificationType.Invitation);
            
            // Добавляем актуальные приглашения
            foreach (var invitation in invitations)
            {
                await AddInvitationNotificationAsync(invitation);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ NOTIFICATIONS: Ошибка загрузки приглашений: {ex.Message}");
        }
    }

    private string GetRoleDisplayName(OrganizationRole role)
    {
        return role switch
        {
            OrganizationRole.Owner => "Владелец",
            OrganizationRole.Admin => "Администратор",
            OrganizationRole.Member => "Участник",
            _ => "Неизвестная роль"
        };
    }

    // ТЕСТОВЫЕ МЕТОДЫ (можно удалить после тестирования)
    public async Task AddTestNotificationsAsync()
    {
        // Информационное уведомление
        var infoNotification = new NotificationItem
        {
            Title = "Добро пожаловать!",
            Message = "Это тестовое информационное уведомление для демонстрации системы.",
            Type = NotificationType.Info,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        // Успешное уведомление
        var successNotification = new NotificationItem
        {
            Title = "Задача выполнена",
            Message = "Ваша задача 'Настройка системы уведомлений' успешно завершена.",
            Type = NotificationType.Success,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            IsRead = false
        };

        // Уведомление об ошибке
        var errorNotification = new NotificationItem
        {
            Title = "Ошибка синхронизации",
            Message = "Не удалось синхронизировать данные с сервером. Попробуйте позже.",
            Type = NotificationType.Error,
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            IsRead = true
        };

        _notifications.AddRange(new[] { infoNotification, successNotification, errorNotification });
        await SaveNotificationsToStorageAsync();
        NotificationsChanged?.Invoke();
    }

    public async Task AddTestInvitationNotificationAsync(NotificationItem invitation)
    {
        Console.WriteLine($"🔔 NOTIFICATIONS: Добавляем тестовое приглашение: {invitation.Title}");
        Console.WriteLine($"🔔 NOTIFICATIONS: ID приглашения: {invitation.Id}");
        Console.WriteLine($"🔔 NOTIFICATIONS: Тип: {invitation.Type}");
        Console.WriteLine($"🔔 NOTIFICATIONS: Данные: {string.Join(", ", invitation.Data.Keys)}");
        
        _notifications.Add(invitation);
        await SaveNotificationsToStorageAsync();
        NotificationsChanged?.Invoke();
        Console.WriteLine($"🔔 NOTIFICATIONS: Тестовое приглашение добавлено, всего уведомлений: {_notifications.Count}");
        
        // Дополнительная проверка после добавления
        var addedNotification = _notifications.FirstOrDefault(n => n.Id == invitation.Id);
        Console.WriteLine($"🔔 NOTIFICATIONS: Проверка добавленного уведомления: {addedNotification != null}");
    }
} 