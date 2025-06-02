using System.Security.Claims;
using TaskTracker.Api.Services;
using TaskTracker.Models;
using TaskTracker.Models.DTOs;

namespace TaskTracker.Api.Endpoints;

public static class ProjectEndpoints
{
    public static void MapProjectEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/projects")
            .WithTags("Projects")
            .RequireAuthorization("RequireAuthenticatedUser");

        // Получить проекты пользователя
        group.MapGet("/", GetUserProjects)
            .WithName("GetUserProjects")
            .WithSummary("Получить проекты пользователя")
            .WithOpenApi();

        // Получить проект по ID
        group.MapGet("/{projectId}", GetProjectById)
            .WithName("GetProjectById")
            .WithSummary("Получить проект по ID")
            .WithOpenApi();

        // Создать новый проект
        group.MapPost("/", CreateProject)
            .WithName("CreateProject")
            .WithSummary("Создать новый проект")
            .WithOpenApi();

        // Обновить проект
        group.MapPut("/{projectId}", UpdateProject)
            .WithName("UpdateProject")
            .WithSummary("Обновить проект")
            .WithOpenApi();

        // Удалить проект
        group.MapDelete("/{projectId}", DeleteProject)
            .WithName("DeleteProject")
            .WithSummary("Удалить проект")
            .WithOpenApi();

        // Добавить участника к проекту
        group.MapPost("/{projectId}/members", AddMemberToProject)
            .WithName("AddMemberToProject")
            .WithSummary("Добавить участника к проекту")
            .WithOpenApi();

        // Удалить участника из проекта
        group.MapDelete("/{projectId}/members/{memberUserId}", RemoveMemberFromProject)
            .WithName("RemoveMemberFromProject")
            .WithSummary("Удалить участника из проекта")
            .WithOpenApi();

        // Endpoint для получения всех проектов (только для админов)
        group.MapGet("/admin/all", GetAllProjects)
            .WithName("GetAllProjects")
            .WithSummary("Получить все проекты (админ)")
            .WithOpenApi();
    }

    private static async Task<IResult> GetUserProjects(
        IProjectService projectService,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var projects = await projectService.GetUserProjectsAsync(userId);
        return Results.Ok(projects);
    }

    private static async Task<IResult> GetProjectById(
        string projectId,
        IProjectService projectService,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var project = await projectService.GetProjectByIdAsync(projectId, userId);
        if (project == null)
            return Results.NotFound();

        return Results.Ok(project);
    }

    private static async Task<IResult> CreateProject(
        CreateProjectRequest request,
        IProjectService projectService,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var project = await projectService.CreateProjectAsync(request, userId);
        return Results.Created($"/api/projects/{project.Id}", project);
    }

    private static async Task<IResult> UpdateProject(
        string projectId,
        UpdateProjectRequest request,
        IProjectService projectService,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var project = await projectService.UpdateProjectAsync(projectId, request, userId);
        if (project == null)
            return Results.NotFound();

        return Results.Ok(project);
    }

    private static async Task<IResult> DeleteProject(
        string projectId,
        IProjectService projectService,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var success = await projectService.DeleteProjectAsync(projectId, userId);
        if (!success)
            return Results.NotFound();

        return Results.NoContent();
    }

    private static async Task<IResult> AddMemberToProject(
        string projectId,
        AddMemberRequest request,
        IProjectService projectService,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var success = await projectService.AddMemberToProjectAsync(
            projectId, userId, request.UserId, request.Role);

        if (!success)
            return Results.BadRequest("Не удалось добавить участника");

        return Results.Ok();
    }

    private static async Task<IResult> RemoveMemberFromProject(
        string projectId,
        string memberUserId,
        IProjectService projectService,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var success = await projectService.RemoveMemberFromProjectAsync(
            projectId, userId, memberUserId);

        if (!success)
            return Results.BadRequest("Не удалось удалить участника");

        return Results.Ok();
    }

    private static async Task<IResult> GetAllProjects(
        IProjectService projectService,
        ClaimsPrincipal user)
    {
        // TODO: Добавить проверку роли админа
        var projects = await projectService.GetAllProjectsAsync();
        return Results.Ok(projects);
    }
}

/// <summary>
/// DTO для добавления участника к проекту
/// </summary>
public class AddMemberRequest
{
    public string UserId { get; set; } = string.Empty;
    public ProjectRole Role { get; set; } = ProjectRole.Member;
} 