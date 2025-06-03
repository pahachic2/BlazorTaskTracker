using TaskTracker.Models;
using TaskTracker.Models.DTOs;

namespace TaskTracker.Web.Services;

public class OrganizationService
{
    private readonly IApiService _apiService;
    private readonly ILogger<OrganizationService> _logger;

    public OrganizationService(IApiService apiService, ILogger<OrganizationService> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    public async Task<List<Organization>> GetUserOrganizationsAsync()
    {
        try
        {
            _logger.LogInformation("🏢 Загружаем организации пользователя...");
            
            var organizations = await _apiService.GetUserOrganizationsAsync();
            
            if (organizations == null)
            {
                _logger.LogWarning("⚠️ API вернул null для организаций");
                return new List<Organization>();
            }

            var result = organizations.Select(org => new Organization
            {
                Id = org.Id,
                Name = org.Name,
                Description = org.Description,
                Icon = org.Icon,
                Color = org.Color,
                Members = org.Members,
                OwnerId = org.OwnerId,
                ProjectCount = org.ProjectCount,
                CreatedDate = org.CreatedDate
            }).ToList();

            _logger.LogInformation($"✅ Загружено {result.Count} организаций");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Ошибка при загрузке организаций");
            return new List<Organization>();
        }
    }

    public async Task<Organization?> CreateOrganizationAsync(CreateOrganizationRequest request)
    {
        try
        {
            _logger.LogInformation($"🏗️ Создаем организацию: {request.Name}");
            
            var response = await _apiService.CreateOrganizationAsync(request);
            
            if (response == null)
            {
                _logger.LogWarning("⚠️ API вернул null при создании организации");
                return null;
            }

            var organization = new Organization
            {
                Id = response.Id,
                Name = response.Name,
                Description = response.Description,
                Icon = response.Icon,
                Color = response.Color,
                Members = response.Members,
                OwnerId = response.OwnerId,
                ProjectCount = response.ProjectCount,
                CreatedDate = response.CreatedDate
            };

            _logger.LogInformation($"✅ Организация создана: {organization.Name} (ID: {organization.Id})");
            return organization;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ Ошибка при создании организации: {request.Name}");
            return null;
        }
    }

    public async Task<Organization?> UpdateOrganizationAsync(string organizationId, UpdateOrganizationRequest request)
    {
        try
        {
            _logger.LogInformation($"📝 Обновляем организацию: {organizationId}");
            
            var response = await _apiService.UpdateOrganizationAsync(organizationId, request);
            
            if (response == null)
            {
                _logger.LogWarning("⚠️ API вернул null при обновлении организации");
                return null;
            }

            var organization = new Organization
            {
                Id = response.Id,
                Name = response.Name,
                Description = response.Description,
                Icon = response.Icon,
                Color = response.Color,
                Members = response.Members,
                OwnerId = response.OwnerId,
                ProjectCount = response.ProjectCount,
                CreatedDate = response.CreatedDate
            };

            _logger.LogInformation($"✅ Организация обновлена: {organization.Name}");
            return organization;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ Ошибка при обновлении организации: {organizationId}");
            return null;
        }
    }

    public async Task<bool> DeleteOrganizationAsync(string organizationId)
    {
        try
        {
            _logger.LogInformation($"🗑️ Удаляем организацию: {organizationId}");
            
            var success = await _apiService.DeleteOrganizationAsync(organizationId);
            
            if (success)
            {
                _logger.LogInformation($"✅ Организация удалена: {organizationId}");
            }
            else
            {
                _logger.LogWarning($"⚠️ Не удалось удалить организацию: {organizationId}");
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ Ошибка при удалении организации: {organizationId}");
            return false;
        }
    }
} 