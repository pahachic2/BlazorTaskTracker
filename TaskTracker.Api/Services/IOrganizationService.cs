using TaskTracker.Models;
using TaskTracker.Models.DTOs;

namespace TaskTracker.Api.Services;

public interface IOrganizationService
{
    Task<List<OrganizationResponse>> GetUserOrganizationsAsync(string userId);
    Task<OrganizationResponse?> GetOrganizationByIdAsync(string organizationId, string userId);
    Task<OrganizationResponse> CreateOrganizationAsync(CreateOrganizationRequest request, string userId);
    Task<OrganizationResponse?> UpdateOrganizationAsync(string organizationId, UpdateOrganizationRequest request, string userId);
    Task<bool> DeleteOrganizationAsync(string organizationId, string userId);
    Task<bool> AddMemberToOrganizationAsync(string organizationId, string userId, string memberUserId);
    Task<bool> RemoveMemberFromOrganizationAsync(string organizationId, string userId, string memberUserId);
    Task<List<OrganizationResponse>> GetAllOrganizationsAsync(); // Для админа
} 