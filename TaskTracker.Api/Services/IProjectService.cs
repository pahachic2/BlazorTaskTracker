using TaskTracker.Models;
using TaskTracker.Models.DTOs;

namespace TaskTracker.Api.Services;

public interface IProjectService
{
    Task<ProjectResponseDto> CreateProjectAsync(CreateProjectDto createProjectDto, string createdByUserId);
    Task<ProjectResponseDto?> GetProjectByIdAsync(string projectId);
    Task<IEnumerable<ProjectResponseDto>> GetProjectsByUserIdAsync(string userId);
    Task<ProjectResponseDto?> UpdateProjectAsync(string projectId, UpdateProjectDto updateProjectDto, string userId);
    Task<bool> DeleteProjectAsync(string projectId, string userId);
    Task<bool> AddMemberToProjectAsync(string projectId, string userId, string memberUserId);
    Task<bool> RemoveMemberFromProjectAsync(string projectId, string userId, string memberUserId);
    Task<bool> IsUserProjectMemberAsync(string projectId, string userId);
} 