using TaskTracker.Models.DTOs;
using TaskTracker.Models;

namespace TaskTracker.Web.Services;

public class OrganizationInvitationService
{
    private readonly IApiService _apiService;
    private readonly IToastService _toastService;
    private readonly ILogger<OrganizationInvitationService> _logger;

    // Кэш для участников организации
    private readonly Dictionary<string, List<OrganizationMemberResponse>> _membersCache = new();
    private readonly Dictionary<string, List<InvitationResponse>> _invitationsCache = new();

    public OrganizationInvitationService(
        IApiService apiService, 
        IToastService toastService,
        ILogger<OrganizationInvitationService> logger)
    {
        _apiService = apiService;
        _toastService = toastService;
        _logger = logger;
    }

    /// <summary>
    /// Получить участников организации с кэшированием
    /// </summary>
    public async Task<List<OrganizationMemberResponse>> GetOrganizationMembersAsync(string organizationId, bool forceRefresh = false)
    {
        try
        {
            if (!forceRefresh && _membersCache.ContainsKey(organizationId))
            {
                _logger.LogInformation($"👥 Возвращаем участников из кэша для организации {organizationId}");
                return _membersCache[organizationId];
            }

            _logger.LogInformation($"👥 Загружаем участников организации {organizationId}");
            var members = await _apiService.GetOrganizationMembersAsync(organizationId);
            
            _membersCache[organizationId] = members;
            return members;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ Ошибка при загрузке участников организации {organizationId}");
            _toastService.ShowError("Ошибка загрузки", "Не удалось загрузить список участников");
            return new List<OrganizationMemberResponse>();
        }
    }

    /// <summary>
    /// Получить приглашения организации с кэшированием
    /// </summary>
    public async Task<List<InvitationResponse>> GetOrganizationInvitationsAsync(string organizationId, bool forceRefresh = false)
    {
        try
        {
            if (!forceRefresh && _invitationsCache.ContainsKey(organizationId))
            {
                _logger.LogInformation($"📋 Возвращаем приглашения из кэша для организации {organizationId}");
                return _invitationsCache[organizationId];
            }

            _logger.LogInformation($"📋 Загружаем приглашения организации {organizationId}");
            var invitations = await _apiService.GetOrganizationInvitationsAsync(organizationId);
            
            _invitationsCache[organizationId] = invitations;
            return invitations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ Ошибка при загрузке приглашений организации {organizationId}");
            _toastService.ShowError("Ошибка загрузки", "Не удалось загрузить список приглашений");
            return new List<InvitationResponse>();
        }
    }

    /// <summary>
    /// Пригласить пользователя в организацию
    /// </summary>
    public async Task<InvitationResponse?> InviteUserAsync(string organizationId, InviteUserRequest request)
    {
        try
        {
            _logger.LogInformation($"📧 Отправляем приглашение {request.Email} в организацию {organizationId}");
            
            var invitation = await _apiService.InviteUserAsync(organizationId, request);
            
            if (invitation != null)
            {
                // Обновляем кэш приглашений
                InvalidateInvitationsCache(organizationId);
                _logger.LogInformation($"✅ Приглашение отправлено: {invitation.Id}");
            }
            
            return invitation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ Ошибка при отправке приглашения {request.Email}");
            _toastService.ShowError("Ошибка приглашения", ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Отозвать приглашение
    /// </summary>
    public async Task<bool> RevokeInvitationAsync(string invitationId, string organizationId)
    {
        try
        {
            _logger.LogInformation($"🚫 Отзываем приглашение {invitationId}");
            
            var success = await _apiService.RevokeInvitationAsync(invitationId);
            
            if (success)
            {
                // Обновляем кэш приглашений
                InvalidateInvitationsCache(organizationId);
                _logger.LogInformation($"✅ Приглашение {invitationId} отозвано");
            }
            
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ Ошибка при отзыве приглашения {invitationId}");
            _toastService.ShowError("Ошибка отзыва", "Не удалось отозвать приглашение");
            return false;
        }
    }

    /// <summary>
    /// Получить информацию о приглашении по токену
    /// </summary>
    public async Task<InvitationInfoResponse?> GetInvitationInfoAsync(string token)
    {
        try
        {
            _logger.LogInformation($"📋 Получаем информацию о приглашении {token}");
            
            var invitation = await _apiService.GetInvitationInfoAsync(token);
            return invitation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ Ошибка при получении информации о приглашении {token}");
            _toastService.ShowError("Ошибка загрузки", "Не удалось получить информацию о приглашении");
            return null;
        }
    }

    /// <summary>
    /// Принять приглашение
    /// </summary>
    public async Task<AcceptInvitationResponse?> AcceptInvitationAsync(AcceptInvitationRequest request)
    {
        try
        {
            _logger.LogInformation($"✅ Принимаем приглашение {request.Token}");
            
            var result = await _apiService.AcceptInvitationAsync(request);
            
            if (result?.Success == true)
            {
                // Очищаем весь кэш, так как изменились участники
                ClearAllCache();
                _logger.LogInformation($"✅ Приглашение {request.Token} принято");
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ Ошибка при принятии приглашения {request.Token}");
            _toastService.ShowError("Ошибка принятия", ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Отклонить приглашение
    /// </summary>
    public async Task<bool> DeclineInvitationAsync(string token)
    {
        try
        {
            _logger.LogInformation($"❌ Отклоняем приглашение {token}");
            
            var request = new DeclineInvitationRequest { Token = token };
            var success = await _apiService.DeclineInvitationAsync(request);
            
            if (success)
            {
                _logger.LogInformation($"✅ Приглашение {token} отклонено");
            }
            
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ Ошибка при отклонении приглашения {token}");
            _toastService.ShowError("Ошибка отклонения", "Не удалось отклонить приглашение");
            return false;
        }
    }

    /// <summary>
    /// Проверить, является ли email валидным
    /// </summary>
    public bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Получить локализованное название роли
    /// </summary>
    public string GetRoleDisplayName(OrganizationRole role)
    {
        return role switch
        {
            OrganizationRole.Owner => "Владелец",
            OrganizationRole.Admin => "Администратор", 
            OrganizationRole.Member => "Участник",
            _ => "Неизвестная роль"
        };
    }

    /// <summary>
    /// Получить локализованное название статуса приглашения
    /// </summary>
    public string GetInvitationStatusDisplayName(InvitationStatus status)
    {
        return status switch
        {
            InvitationStatus.Pending => "Ожидает",
            InvitationStatus.Accepted => "Принято",
            InvitationStatus.Declined => "Отклонено",
            InvitationStatus.Expired => "Истекло",
            InvitationStatus.Revoked => "Отозвано",
            _ => "Неизвестно"
        };
    }

    /// <summary>
    /// Получить CSS класс для статуса приглашения
    /// </summary>
    public string GetInvitationStatusCssClass(InvitationStatus status)
    {
        return status switch
        {
            InvitationStatus.Pending => "bg-yellow-100 text-yellow-800",
            InvitationStatus.Accepted => "bg-green-100 text-green-800",
            InvitationStatus.Declined => "bg-red-100 text-red-800",
            InvitationStatus.Expired => "bg-gray-100 text-gray-800",
            InvitationStatus.Revoked => "bg-red-100 text-red-800",
            _ => "bg-gray-100 text-gray-800"
        };
    }

    /// <summary>
    /// Очистить кэш участников для организации
    /// </summary>
    public void InvalidateMembersCache(string organizationId)
    {
        if (_membersCache.ContainsKey(organizationId))
        {
            _membersCache.Remove(organizationId);
            _logger.LogInformation($"🗑️ Кэш участников для организации {organizationId} очищен");
        }
    }

    /// <summary>
    /// Очистить кэш приглашений для организации
    /// </summary>
    public void InvalidateInvitationsCache(string organizationId)
    {
        if (_invitationsCache.ContainsKey(organizationId))
        {
            _invitationsCache.Remove(organizationId);
            _logger.LogInformation($"🗑️ Кэш приглашений для организации {organizationId} очищен");
        }
    }

    /// <summary>
    /// Очистить весь кэш
    /// </summary>
    public void ClearAllCache()
    {
        _membersCache.Clear();
        _invitationsCache.Clear();
        _logger.LogInformation($"🗑️ Весь кэш приглашений очищен");
    }
} 