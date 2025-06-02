using TaskTracker.Models;

namespace TaskTracker.Web.Services;

/// <summary>
/// Интерфейс для работы с локальным хранилищем браузера
/// </summary>
public interface ILocalStorageService
{
    /// <summary>
    /// Сохраняет значение в localStorage
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <param name="value">Значение</param>
    Task SetItemAsync(string key, string value);
    
    /// <summary>
    /// Получает значение из localStorage
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>Значение или null, если ключ не найден</returns>
    Task<string?> GetItemAsync(string key);
    
    /// <summary>
    /// Удаляет элемент из localStorage
    /// </summary>
    /// <param name="key">Ключ</param>
    Task RemoveItemAsync(string key);
    
    /// <summary>
    /// Очищает все данные из localStorage
    /// </summary>
    Task ClearAsync();
    
    /// <summary>
    /// Проверяет существование ключа в localStorage
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>True, если ключ существует</returns>
    Task<bool> ContainsKeyAsync(string key);
    
    /// <summary>
    /// Сохраняет токен авторизации
    /// </summary>
    /// <param name="token">Токен</param>
    Task SetAuthTokenAsync(string token);
    
    /// <summary>
    /// Получает токен авторизации
    /// </summary>
    /// <returns>Токен или null</returns>
    Task<string?> GetAuthTokenAsync();
    
    /// <summary>
    /// Сохраняет данные пользователя
    /// </summary>
    /// <param name="username">Имя пользователя</param>
    /// <param name="email">Email</param>
    Task SetUserDataAsync(string username, string email);
    
    /// <summary>
    /// Получает имя пользователя
    /// </summary>
    /// <returns>Имя пользователя или null</returns>
    Task<string?> GetUsernameAsync();
    
    /// <summary>
    /// Получает email пользователя
    /// </summary>
    /// <returns>Email или null</returns>
    Task<string?> GetEmailAsync();
    
    /// <summary>
    /// Получает все данные пользователя
    /// </summary>
    /// <returns>Объект UserData с данными пользователя</returns>
    Task<UserData> GetUserDataAsync();
    
    /// <summary>
    /// Очищает все данные авторизации
    /// </summary>
    Task ClearAuthDataAsync();
} 