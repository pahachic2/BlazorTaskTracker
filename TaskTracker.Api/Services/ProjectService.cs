using MongoDB.Bson;
using MongoDB.Driver;
using TaskTracker.Models;
using TaskTracker.Models.DTOs;

namespace TaskTracker.Api.Services;

public class ProjectService : IProjectService
{
    private readonly IDatabaseService<Project> _projectRepository;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(IDatabaseService<Project> projectRepository, ILogger<ProjectService> logger)
    {
        _projectRepository = projectRepository;
        _logger = logger;
    }

    public async Task<ProjectResponseDto> CreateProjectAsync(CreateProjectDto createProjectDto, string createdByUserId)
    {
        try
        {
            _logger.LogInformation("Создание нового проекта для пользователя {UserId}", createdByUserId);

            var project = new Project
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = createProjectDto.Name,
                Description = createProjectDto.Description ?? "",
                Icon = createProjectDto.Icon,
                Color = createProjectDto.Color,
                CreatedDate = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Members = new List<string> { createdByUserId }, // Создатель автоматически становится участником
                TaskCount = 0,
                IsActive = true
            };

            // Добавляем дополнительных участников если они указаны
            if (createProjectDto.Members?.Any() == true)
            {
                foreach (var member in createProjectDto.Members)
                {
                    if (!project.Members.Contains(member))
                    {
                        project.Members.Add(member);
                    }
                }
            }

            var createdProject = await _projectRepository.CreateAsync(project);
            
            _logger.LogInformation("Проект {ProjectName} успешно создан с ID {ProjectId}", 
                createdProject.Name, createdProject.Id);

            return MapToResponseDto(createdProject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании проекта для пользователя {UserId}", createdByUserId);
            throw;
        }
    }

    public async Task<ProjectResponseDto?> GetProjectByIdAsync(string projectId)
    {
        try
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            return project != null ? MapToResponseDto(project) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении проекта {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<IEnumerable<ProjectResponseDto>> GetProjectsByUserIdAsync(string userId)
    {
        try
        {
            var projects = await _projectRepository.FindAsync(p => p.Members.Contains(userId) && p.IsActive);
            return projects.Select(MapToResponseDto).OrderByDescending(p => p.UpdatedAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении проектов пользователя {UserId}", userId);
            throw;
        }
    }

    public async Task<ProjectResponseDto?> UpdateProjectAsync(string projectId, UpdateProjectDto updateProjectDto, string userId)
    {
        try
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
                return null;

            // Проверяем права доступа
            if (!project.Members.Contains(userId))
                return null;

            // Обновляем поля если они указаны
            if (!string.IsNullOrEmpty(updateProjectDto.Name))
                project.Name = updateProjectDto.Name;

            if (updateProjectDto.Description != null)
                project.Description = updateProjectDto.Description;

            if (!string.IsNullOrEmpty(updateProjectDto.Icon))
                project.Icon = updateProjectDto.Icon;

            if (!string.IsNullOrEmpty(updateProjectDto.Color))
                project.Color = updateProjectDto.Color;

            if (updateProjectDto.Members != null)
            {
                // Создатель проекта всегда должен остаться участником
                var currentMembers = project.Members.ToList();
                project.Members = updateProjectDto.Members.ToList();
                
                // Убеждаемся что создатель остается в списке
                if (!project.Members.Contains(userId))
                    project.Members.Add(userId);
            }

            if (updateProjectDto.IsActive.HasValue)
                project.IsActive = updateProjectDto.IsActive.Value;

            project.UpdatedAt = DateTime.UtcNow;

            var updatedProject = await _projectRepository.UpdateAsync(projectId, project);
            
            _logger.LogInformation("Проект {ProjectId} обновлен пользователем {UserId}", projectId, userId);
            
            return MapToResponseDto(updatedProject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении проекта {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<bool> DeleteProjectAsync(string projectId, string userId)
    {
        try
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null || !project.Members.Contains(userId))
                return false;

            // Мягкое удаление - помечаем как неактивный
            project.IsActive = false;
            project.UpdatedAt = DateTime.UtcNow;
            
            await _projectRepository.UpdateAsync(projectId, project);
            
            _logger.LogInformation("Проект {ProjectId} деактивирован пользователем {UserId}", projectId, userId);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении проекта {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<bool> AddMemberToProjectAsync(string projectId, string userId, string memberUserId)
    {
        try
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null || !project.Members.Contains(userId))
                return false;

            if (!project.Members.Contains(memberUserId))
            {
                project.Members.Add(memberUserId);
                project.UpdatedAt = DateTime.UtcNow;
                
                await _projectRepository.UpdateAsync(projectId, project);
                
                _logger.LogInformation("Пользователь {MemberUserId} добавлен в проект {ProjectId}", 
                    memberUserId, projectId);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при добавлении участника в проект {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<bool> RemoveMemberFromProjectAsync(string projectId, string userId, string memberUserId)
    {
        try
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null || !project.Members.Contains(userId))
                return false;

            if (project.Members.Contains(memberUserId) && project.Members.Count > 1)
            {
                project.Members.Remove(memberUserId);
                project.UpdatedAt = DateTime.UtcNow;
                
                await _projectRepository.UpdateAsync(projectId, project);
                
                _logger.LogInformation("Пользователь {MemberUserId} удален из проекта {ProjectId}", 
                    memberUserId, projectId);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении участника из проекта {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<bool> IsUserProjectMemberAsync(string projectId, string userId)
    {
        try
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            return project?.Members.Contains(userId) == true && project.IsActive;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при проверке участия пользователя {UserId} в проекте {ProjectId}", 
                userId, projectId);
            throw;
        }
    }

    private static ProjectResponseDto MapToResponseDto(Project project)
    {
        return new ProjectResponseDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Color = project.Color,
            Icon = project.Icon,
            CreatedDate = project.CreatedDate,
            Members = project.Members,
            TaskCount = project.TaskCount,
            IsActive = project.IsActive,
            UpdatedAt = project.UpdatedAt
        };
    }
} 