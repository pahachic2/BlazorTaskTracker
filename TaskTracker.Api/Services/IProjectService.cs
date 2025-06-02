using TaskTracker.Models;
using TaskTracker.Models.DTOs;

namespace TaskTracker.Api.Services;

public interface IProjectService
{
    Task<List<ProjectResponse>> GetUserProjectsAsync(string userId);
    Task<ProjectResponse?> GetProjectByIdAsync(string projectId, string userId);
    Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest request, string userId);
    Task<ProjectResponse?> UpdateProjectAsync(string projectId, UpdateProjectRequest request, string userId);
    Task<bool> DeleteProjectAsync(string projectId, string userId);
    Task<bool> AddMemberToProjectAsync(string projectId, string userId, string memberUserId, ProjectRole role);
    Task<bool> RemoveMemberFromProjectAsync(string projectId, string userId, string memberUserId);
    Task<List<ProjectResponse>> GetAllProjectsAsync(); // Для админа
} 