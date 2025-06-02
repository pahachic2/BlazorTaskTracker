using MongoDB.Driver;
using TaskTracker.Models;
using TaskTracker.Models.DTOs;

namespace TaskTracker.Api.Services;

public class ProjectService : IProjectService
{
    private readonly IDatabaseService<Project> _projectDatabase;
    private readonly IDatabaseService<UserProject> _userProjectDatabase;

    public ProjectService(
        IDatabaseService<Project> projectDatabase,
        IDatabaseService<UserProject> userProjectDatabase)
    {
        _projectDatabase = projectDatabase;
        _userProjectDatabase = userProjectDatabase;
    }

    public async Task<List<ProjectResponse>> GetUserProjectsAsync(string userId)
    {
        // Получаем все связи пользователя с проектами
        var userProjects = await _userProjectDatabase.FindAsync(
            up => up.UserId == userId && up.IsActive);

        if (!userProjects.Any())
            return new List<ProjectResponse>();

        var projectIds = userProjects.Select(up => up.ProjectId).ToList();
        
        // Получаем сами проекты
        var projects = await _projectDatabase.FindAsync(
            p => projectIds.Contains(p.Id) && p.IsActive);

        return projects.Select(MapToResponse).ToList();
    }

    public async Task<ProjectResponse?> GetProjectByIdAsync(string projectId, string userId)
    {
        // Проверяем, что пользователь имеет доступ к проекту
        var userProjects = await _userProjectDatabase.FindAsync(
            up => up.ProjectId == projectId && up.UserId == userId && up.IsActive);
        
        var userProject = userProjects.FirstOrDefault();
        if (userProject == null)
            return null;

        var project = await _projectDatabase.GetByIdAsync(projectId);
        return project != null ? MapToResponse(project) : null;
    }

    public async Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest request, string userId)
    {
        var project = new Project
        {
            Name = request.Name,
            Description = request.Description ?? string.Empty,
            Icon = request.Icon,
            Color = request.Color,
            OwnerId = userId,
            CreatedDate = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            TaskCount = 0,
            Members = new List<string> { userId }
        };

        await _projectDatabase.CreateAsync(project);

        // Создаем связь пользователя с проектом как владельца
        var userProject = new UserProject
        {
            UserId = userId,
            ProjectId = project.Id,
            Role = ProjectRole.Owner,
            JoinedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _userProjectDatabase.CreateAsync(userProject);

        return MapToResponse(project);
    }

    public async Task<ProjectResponse?> UpdateProjectAsync(string projectId, UpdateProjectRequest request, string userId)
    {
        // Проверяем, что пользователь является владельцем или админом
        var userProjects = await _userProjectDatabase.FindAsync(
            up => up.ProjectId == projectId && up.UserId == userId && up.IsActive &&
                  (up.Role == ProjectRole.Owner || up.Role == ProjectRole.Admin));
        
        var userProject = userProjects.FirstOrDefault();
        if (userProject == null)
            return null;

        var project = await _projectDatabase.GetByIdAsync(projectId);
        if (project == null)
            return null;

        project.Name = request.Name;
        project.Description = request.Description ?? string.Empty;
        project.Icon = request.Icon;
        project.Color = request.Color;
        project.IsActive = request.IsActive;
        project.UpdatedAt = DateTime.UtcNow;

        await _projectDatabase.UpdateAsync(projectId, project);
        return MapToResponse(project);
    }

    public async Task<bool> DeleteProjectAsync(string projectId, string userId)
    {
        // Проверяем, что пользователь является владельцем
        var userProjects = await _userProjectDatabase.FindAsync(
            up => up.ProjectId == projectId && up.UserId == userId && up.IsActive &&
                  up.Role == ProjectRole.Owner);
        
        var userProject = userProjects.FirstOrDefault();
        if (userProject == null)
            return false;

        var project = await _projectDatabase.GetByIdAsync(projectId);
        if (project == null)
            return false;

        // Мягкое удаление - помечаем как неактивный
        project.IsActive = false;
        project.UpdatedAt = DateTime.UtcNow;

        await _projectDatabase.UpdateAsync(projectId, project);
        return true;
    }

    public async Task<bool> AddMemberToProjectAsync(string projectId, string userId, string memberUserId, ProjectRole role)
    {
        // Проверяем, что пользователь является владельцем или админом
        var userProjects = await _userProjectDatabase.FindAsync(
            up => up.ProjectId == projectId && up.UserId == userId && up.IsActive &&
                  (up.Role == ProjectRole.Owner || up.Role == ProjectRole.Admin));
        
        var userProject = userProjects.FirstOrDefault();
        if (userProject == null)
            return false;

        // Проверяем, не является ли пользователь уже участником
        var existingMembers = await _userProjectDatabase.FindAsync(
            up => up.ProjectId == projectId && up.UserId == memberUserId);
        
        var existingMember = existingMembers.FirstOrDefault();
        if (existingMember != null)
        {
            // Обновляем роль существующего участника
            existingMember.Role = role;
            existingMember.IsActive = true;
            await _userProjectDatabase.UpdateAsync(existingMember.Id, existingMember);
        }
        else
        {
            // Добавляем нового участника
            var newUserProject = new UserProject
            {
                UserId = memberUserId,
                ProjectId = projectId,
                Role = role,
                JoinedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _userProjectDatabase.CreateAsync(newUserProject);
        }

        return true;
    }

    public async Task<bool> RemoveMemberFromProjectAsync(string projectId, string userId, string memberUserId)
    {
        // Проверяем, что пользователь является владельцем или админом
        var userProjects = await _userProjectDatabase.FindAsync(
            up => up.ProjectId == projectId && up.UserId == userId && up.IsActive &&
                  (up.Role == ProjectRole.Owner || up.Role == ProjectRole.Admin));
        
        var userProject = userProjects.FirstOrDefault();
        if (userProject == null)
            return false;

        var memberProjects = await _userProjectDatabase.FindAsync(
            up => up.ProjectId == projectId && up.UserId == memberUserId && up.IsActive);
        
        var memberProject = memberProjects.FirstOrDefault();
        if (memberProject == null)
            return false;

        // Владельца нельзя удалить
        if (memberProject.Role == ProjectRole.Owner)
            return false;

        memberProject.IsActive = false;
        await _userProjectDatabase.UpdateAsync(memberProject.Id, memberProject);

        return true;
    }

    public async Task<List<ProjectResponse>> GetAllProjectsAsync()
    {
        var projects = await _projectDatabase.GetAllAsync();
        return projects.Select(MapToResponse).ToList();
    }

    private static ProjectResponse MapToResponse(Project project)
    {
        return new ProjectResponse
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
            OwnerId = project.OwnerId,
            UpdatedAt = project.UpdatedAt
        };
    }
} 