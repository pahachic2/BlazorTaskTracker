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
    Task<InvitationResponse> InviteUserAsync(string organizationId, InviteUserRequest request, string userId);
    Task<List<InvitationResponse>> GetOrganizationInvitationsAsync(string organizationId, string userId);
    Task<List<OrganizationMemberResponse>> GetOrganizationMembersAsync(string organizationId, string userId);
    Task<bool> RevokeInvitationAsync(string invitationId, string userId);
    Task<AcceptInvitationResponse> AcceptInvitationAsync(AcceptInvitationRequest request);
    Task<InvitationInfoResponse> GetInvitationInfoAsync(string token);
    Task<bool> DeclineInvitationAsync(DeclineInvitationRequest request);
} 