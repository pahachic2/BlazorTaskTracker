using System.ComponentModel.DataAnnotations;

namespace TaskTracker.Models.DTOs;

/// <summary>
/// DTO для отправки приглашения в организацию
/// </summary>
public class InviteUserRequest
{
    [Required(ErrorMessage = "Email обязателен")]
    [EmailAddress(ErrorMessage = "Некорректный формат email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Роль обязательна")]
    public OrganizationRole Role { get; set; } = OrganizationRole.Member;

    [StringLength(500, ErrorMessage = "Сообщение не должно превышать 500 символов")]
    public string? Message { get; set; }
}

/// <summary>
/// DTO для ответа с информацией о приглашении
/// </summary>
public class InvitationResponse
{
    public string Id { get; set; } = string.Empty;
    public string OrganizationId { get; set; } = string.Empty;
    public string OrganizationName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string InvitedBy { get; set; } = string.Empty;
    public string InvitedByName { get; set; } = string.Empty;
    public OrganizationRole Role { get; set; }
    public InvitationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    
    // Добавляем токен для принятия приглашения из интерфейса
    public string Token { get; set; } = string.Empty;
    
    // Указывает, был ли отправлен email (для незарегистрированных) или только уведомление в интерфейс (для зарегистрированных)
    public bool EmailSent { get; set; } = false;
    
    // Указывает, был ли пользователь зарегистрирован на момент отправки приглашения
    public bool UserWasRegistered { get; set; } = false;
}

/// <summary>
/// DTO для принятия приглашения
/// </summary>
public class AcceptInvitationRequest
{
    [Required(ErrorMessage = "Токен приглашения обязателен")]
    public string Token { get; set; } = string.Empty;

    // Для авторизованных пользователей (автоматически заполняется из JWT)
    public string? UserId { get; set; }

    // Если пользователь еще не зарегистрирован
    [StringLength(50, ErrorMessage = "Имя пользователя не должно превышать 50 символов")]
    public string? Username { get; set; }

    [StringLength(100, ErrorMessage = "Пароль не должен превышать 100 символов")]
    public string? Password { get; set; }
}

/// <summary>
/// DTO для ответа при принятии приглашения
/// </summary>
public class AcceptInvitationResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? Token { get; set; } // JWT токен для автоматического входа
    public OrganizationResponse? Organization { get; set; }
}

/// <summary>
/// DTO для отклонения приглашения
/// </summary>
public class DeclineInvitationRequest
{
    [Required(ErrorMessage = "Токен приглашения обязателен")]
    public string Token { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Причина не должна превышать 500 символов")]
    public string? Reason { get; set; }
}

/// <summary>
/// DTO для получения информации о приглашении по токену
/// </summary>
public class InvitationInfoResponse
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? OrganizationName { get; set; }
    public string? InvitedByName { get; set; }
    public OrganizationRole? Role { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool RequiresRegistration { get; set; } // true если email не зарегистрирован
}

/// <summary>
/// DTO для получения участников организации
/// </summary>
public class OrganizationMemberResponse
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public OrganizationRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// DTO для ответа на поиск пользователя
/// </summary>
public class UserSearchResponse
{
    public bool Found { get; set; }
    public UserSearchResult? User { get; set; }
}

/// <summary>
/// DTO для результата поиска пользователя
/// </summary>
public class UserSearchResult
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsAlreadyMember { get; set; }
} 