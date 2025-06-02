namespace TaskTracker.Models;

/// <summary>
/// Модель данных пользователя из локального хранилища
/// </summary>
public class UserData
{
    public string? Token { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    
    /// <summary>
    /// Проверяет, авторизован ли пользователь
    /// </summary>
    public bool IsAuthenticated => !string.IsNullOrEmpty(Token);
    
    /// <summary>
    /// Создает пустой объект данных пользователя
    /// </summary>
    public static UserData Empty => new UserData();
} 