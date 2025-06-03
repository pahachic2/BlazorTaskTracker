using MongoDB.Driver;
using TaskTracker.Models;
using TaskTracker.Models.DTOs;

namespace TaskTracker.Api.Services;

public class OrganizationService : IOrganizationService
{
    private readonly IDatabaseService<Organization> _organizationDatabase;
    private readonly IDatabaseService<UserOrganization> _userOrganizationDatabase;
    private readonly IDatabaseService<Project> _projectDatabase;

    public OrganizationService(
        IDatabaseService<Organization> organizationDatabase,
        IDatabaseService<UserOrganization> userOrganizationDatabase,
        IDatabaseService<Project> projectDatabase)
    {
        _organizationDatabase = organizationDatabase;
        _userOrganizationDatabase = userOrganizationDatabase;
        _projectDatabase = projectDatabase;
    }

    public async Task<List<OrganizationResponse>> GetUserOrganizationsAsync(string userId)
    {
        // Получаем все связи пользователя с организациями
        var userOrganizations = await _userOrganizationDatabase.FindAsync(
            uo => uo.UserId == userId && uo.IsActive);

        if (!userOrganizations.Any())
            return new List<OrganizationResponse>();

        var organizationIds = userOrganizations.Select(uo => uo.OrganizationId).ToList();
        
        // Получаем сами организации
        var organizations = await _organizationDatabase.FindAsync(
            o => organizationIds.Contains(o.Id));

        var result = new List<OrganizationResponse>();
        foreach (var org in organizations)
        {
            var response = await MapToResponseAsync(org);
            result.Add(response);
        }

        return result;
    }

    public async Task<OrganizationResponse?> GetOrganizationByIdAsync(string organizationId, string userId)
    {
        // Проверяем, что пользователь имеет доступ к организации
        var userOrganizations = await _userOrganizationDatabase.FindAsync(
            uo => uo.OrganizationId == organizationId && uo.UserId == userId && uo.IsActive);
        
        var userOrganization = userOrganizations.FirstOrDefault();
        if (userOrganization == null)
            return null;

        var organization = await _organizationDatabase.GetByIdAsync(organizationId);
        return organization != null ? await MapToResponseAsync(organization) : null;
    }

    public async Task<OrganizationResponse> CreateOrganizationAsync(CreateOrganizationRequest request, string userId)
    {
        var organization = new Organization
        {
            Name = request.Name,
            Description = request.Description ?? string.Empty,
            Icon = request.Icon,
            Color = request.Color,
            OwnerId = userId,
            CreatedDate = DateTime.UtcNow,
            ProjectCount = 0,
            Members = new List<string> { userId }
        };

        await _organizationDatabase.CreateAsync(organization);

        // Создаем связь пользователя с организацией как владельца
        var userOrganization = new UserOrganization
        {
            UserId = userId,
            OrganizationId = organization.Id,
            Role = OrganizationRole.Owner,
            JoinedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _userOrganizationDatabase.CreateAsync(userOrganization);

        return await MapToResponseAsync(organization);
    }

    public async Task<OrganizationResponse?> UpdateOrganizationAsync(string organizationId, UpdateOrganizationRequest request, string userId)
    {
        // Проверяем, что пользователь является владельцем или админом
        var userOrganizations = await _userOrganizationDatabase.FindAsync(
            uo => uo.OrganizationId == organizationId && uo.UserId == userId && uo.IsActive &&
                  (uo.Role == OrganizationRole.Owner || uo.Role == OrganizationRole.Admin));
        
        var userOrganization = userOrganizations.FirstOrDefault();
        if (userOrganization == null)
            return null;

        var organization = await _organizationDatabase.GetByIdAsync(organizationId);
        if (organization == null)
            return null;

        organization.Name = request.Name;
        organization.Description = request.Description ?? string.Empty;
        organization.Icon = request.Icon;
        organization.Color = request.Color;

        await _organizationDatabase.UpdateAsync(organizationId, organization);
        return await MapToResponseAsync(organization);
    }

    public async Task<bool> DeleteOrganizationAsync(string organizationId, string userId)
    {
        // Проверяем, что пользователь является владельцем
        var userOrganizations = await _userOrganizationDatabase.FindAsync(
            uo => uo.OrganizationId == organizationId && uo.UserId == userId && uo.IsActive &&
                  uo.Role == OrganizationRole.Owner);
        
        var userOrganization = userOrganizations.FirstOrDefault();
        if (userOrganization == null)
            return false;

        var organization = await _organizationDatabase.GetByIdAsync(organizationId);
        if (organization == null)
            return false;

        // Удаляем организацию
        await _organizationDatabase.DeleteAsync(organizationId);
        return true;
    }

    public async Task<bool> AddMemberToOrganizationAsync(string organizationId, string userId, string memberUserId)
    {
        // Проверяем, что пользователь является владельцем или админом
        var userOrganizations = await _userOrganizationDatabase.FindAsync(
            uo => uo.OrganizationId == organizationId && uo.UserId == userId && uo.IsActive &&
                  (uo.Role == OrganizationRole.Owner || uo.Role == OrganizationRole.Admin));
        
        var userOrganization = userOrganizations.FirstOrDefault();
        if (userOrganization == null)
            return false;

        // Проверяем, не является ли пользователь уже участником
        var existingMembers = await _userOrganizationDatabase.FindAsync(
            uo => uo.OrganizationId == organizationId && uo.UserId == memberUserId);
        
        var existingMember = existingMembers.FirstOrDefault();
        if (existingMember != null)
        {
            // Обновляем статус существующего участника
            existingMember.IsActive = true;
            await _userOrganizationDatabase.UpdateAsync(existingMember.Id, existingMember);
        }
        else
        {
            // Добавляем нового участника
            var newUserOrganization = new UserOrganization
            {
                UserId = memberUserId,
                OrganizationId = organizationId,
                Role = OrganizationRole.Member,
                JoinedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _userOrganizationDatabase.CreateAsync(newUserOrganization);
        }

        return true;
    }

    public async Task<bool> RemoveMemberFromOrganizationAsync(string organizationId, string userId, string memberUserId)
    {
        // Проверяем, что пользователь является владельцем или админом
        var userOrganizations = await _userOrganizationDatabase.FindAsync(
            uo => uo.OrganizationId == organizationId && uo.UserId == userId && uo.IsActive &&
                  (uo.Role == OrganizationRole.Owner || uo.Role == OrganizationRole.Admin));
        
        var userOrganization = userOrganizations.FirstOrDefault();
        if (userOrganization == null)
            return false;

        var memberOrganizations = await _userOrganizationDatabase.FindAsync(
            uo => uo.OrganizationId == organizationId && uo.UserId == memberUserId && uo.IsActive);
        
        var memberOrganization = memberOrganizations.FirstOrDefault();
        if (memberOrganization == null)
            return false;

        // Владельца нельзя удалить
        if (memberOrganization.Role == OrganizationRole.Owner)
            return false;

        memberOrganization.IsActive = false;
        await _userOrganizationDatabase.UpdateAsync(memberOrganization.Id, memberOrganization);

        return true;
    }

    public async Task<List<OrganizationResponse>> GetAllOrganizationsAsync()
    {
        var organizations = await _organizationDatabase.GetAllAsync();
        var result = new List<OrganizationResponse>();
        
        foreach (var org in organizations)
        {
            var response = await MapToResponseAsync(org);
            result.Add(response);
        }

        return result;
    }

    private async Task<OrganizationResponse> MapToResponseAsync(Organization organization)
    {
        // Подсчитываем количество проектов в организации
        var projects = await _projectDatabase.FindAsync(p => p.OrganizationId == organization.Id && p.IsActive);
        
        return new OrganizationResponse
        {
            Id = organization.Id,
            Name = organization.Name,
            Description = organization.Description,
            Icon = organization.Icon,
            Color = organization.Color,
            Members = organization.Members,
            OwnerId = organization.OwnerId,
            ProjectCount = projects.Count(),
            CreatedDate = organization.CreatedDate
        };
    }
} 