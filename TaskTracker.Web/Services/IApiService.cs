using TaskTracker.Models;
using TaskTracker.Models.DTOs;

namespace TaskTracker.Web.Services;

public interface IApiService
{
    // Методы аутентификации
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
    
    // Методы для организаций
    Task<List<OrganizationResponse>> GetUserOrganizationsAsync();
    Task<OrganizationResponse?> GetOrganizationByIdAsync(string organizationId);
    Task<OrganizationResponse?> CreateOrganizationAsync(CreateOrganizationRequest request);
    Task<OrganizationResponse?> UpdateOrganizationAsync(string organizationId, UpdateOrganizationRequest request);
    Task<bool> DeleteOrganizationAsync(string organizationId);
    
    // Методы для проектов
    Task<List<ProjectResponse>> GetUserProjectsAsync();
    Task<ProjectResponse?> GetProjectByIdAsync(string projectId);
    Task<ProjectResponse?> CreateProjectAsync(CreateProjectRequest request);
    Task<ProjectResponse?> UpdateProjectAsync(string projectId, UpdateProjectRequest request);
    Task<bool> DeleteProjectAsync(string projectId);
    
    // Методы для канбан доски
    Task<List<ColumnResponse>> GetProjectColumnsAsync(string projectId);
    Task<ColumnResponse?> CreateColumnAsync(CreateColumnRequest request);
    Task<ColumnResponse?> UpdateColumnAsync(string columnId, UpdateColumnRequest request);
    Task<bool> DeleteColumnAsync(string columnId);
    
    // Методы для задач
    Task<List<TaskResponse>> GetColumnTasksAsync(string columnId);
    Task<TaskResponse?> GetTaskByIdAsync(string taskId);
    Task<TaskResponse?> CreateTaskAsync(CreateTaskRequest request);
    Task<TaskResponse?> UpdateTaskAsync(string taskId, UpdateTaskRequest request);
    Task<bool> DeleteTaskAsync(string taskId);
    Task<TaskResponse?> MoveTaskAsync(string taskId, MoveTaskRequest request);

    // НОВЫЕ МЕТОДЫ ДЛЯ СИСТЕМЫ ПРИГЛАШЕНИЙ
    
    // Участники организации
    Task<List<OrganizationMemberResponse>> GetOrganizationMembersAsync(string organizationId);

    // Приглашения (авторизованные методы)
    Task<InvitationResponse?> InviteUserAsync(string organizationId, InviteUserRequest request);
    Task<List<InvitationResponse>> GetOrganizationInvitationsAsync(string organizationId);
    Task<bool> RevokeInvitationAsync(string invitationId);

    // Публичные методы для обработки приглашений
    Task<InvitationInfoResponse?> GetInvitationInfoAsync(string token);
    Task<AcceptInvitationResponse?> AcceptInvitationAsync(AcceptInvitationRequest request);
    Task<bool> DeclineInvitationAsync(DeclineInvitationRequest request);

    // НОВЫЙ МЕТОД: Получить приглашения текущего пользователя
    Task<List<InvitationResponse>> GetUserInvitationsAsync();

    // Поиск и добавление пользователей
    Task<UserSearchResponse?> SearchUserByEmailAsync(string organizationId, string email);
    Task<bool> AddExistingUserAsync(string organizationId, string userId, OrganizationRole role = OrganizationRole.Member);
} 