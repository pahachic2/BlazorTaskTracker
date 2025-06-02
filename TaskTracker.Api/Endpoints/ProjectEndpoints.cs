using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskTracker.Api.Services;
using TaskTracker.Models.DTOs;

namespace TaskTracker.Api.Endpoints;

public class ProjectEndpoints : IEndpointGroup
{
    public void MapEndpoints(WebApplication app)
    {
        var projectsGroup = app.MapGroup("/api/projects")
            .WithTags("Projects")
            .RequireAuthorization();

        // Создание нового проекта
        projectsGroup.MapPost("/", CreateProject)
            .WithName("CreateProject")
            .WithSummary("Создать новый проект")
            .ProducesValidationProblem()
            .Produces<ProjectResponseDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        // Получение всех проектов пользователя
        projectsGroup.MapGet("/", GetUserProjects)
            .WithName("GetUserProjects")
            .WithSummary("Получить все проекты пользователя")
            .Produces<IEnumerable<ProjectResponseDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        // Получение проекта по ID
        projectsGroup.MapGet("/{projectId}", GetProjectById)
            .WithName("GetProjectById")
            .WithSummary("Получить проект по ID")
            .Produces<ProjectResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        // Обновление проекта
        projectsGroup.MapPut("/{projectId}", UpdateProject)
            .WithName("UpdateProject")
            .WithSummary("Обновить проект")
            .ProducesValidationProblem()
            .Produces<ProjectResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        // Удаление проекта
        projectsGroup.MapDelete("/{projectId}", DeleteProject)
            .WithName("DeleteProject")
            .WithSummary("Удалить проект")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        // Добавление участника в проект
        projectsGroup.MapPost("/{projectId}/members/{memberUserId}", AddMemberToProject)
            .WithName("AddMemberToProject")
            .WithSummary("Добавить участника в проект")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        // Удаление участника из проекта
        projectsGroup.MapDelete("/{projectId}/members/{memberUserId}", RemoveMemberFromProject)
            .WithName("RemoveMemberFromProject")
            .WithSummary("Удалить участника из проекта")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
    }

    private static async Task<IResult> CreateProject(
        [FromBody] CreateProjectDto createProjectDto,
        IProjectService projectService,
        ClaimsPrincipal user,
        ILogger<ProjectEndpoints> logger)
    {
        try
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                logger.LogWarning("Попытка создания проекта без действительного ID пользователя");
                return Results.Unauthorized();
            }

            var project = await projectService.CreateProjectAsync(createProjectDto, userId);
            
            logger.LogInformation("Проект {ProjectName} создан пользователем {UserId}", 
                project.Name, userId);
            
            return Results.Created($"/api/projects/{project.Id}", project);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при создании проекта");
            return Results.Problem("Внутренняя ошибка сервера", statusCode: 500);
        }
    }

    private static async Task<IResult> GetUserProjects(
        IProjectService projectService,
        ClaimsPrincipal user,
        ILogger<ProjectEndpoints> logger)
    {
        try
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                logger.LogWarning("Попытка получения проектов без действительного ID пользователя");
                return Results.Unauthorized();
            }

            var projects = await projectService.GetProjectsByUserIdAsync(userId);
            return Results.Ok(projects);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при получении проектов пользователя");
            return Results.Problem("Внутренняя ошибка сервера", statusCode: 500);
        }
    }

    private static async Task<IResult> GetProjectById(
        string projectId,
        IProjectService projectService,
        ClaimsPrincipal user,
        ILogger<ProjectEndpoints> logger)
    {
        try
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Unauthorized();
            }

            // Проверяем является ли пользователь участником проекта
            var isMember = await projectService.IsUserProjectMemberAsync(projectId, userId);
            if (!isMember)
            {
                logger.LogWarning("Пользователь {UserId} попытался получить доступ к проекту {ProjectId} без прав доступа", 
                    userId, projectId);
                return Results.Forbid();
            }

            var project = await projectService.GetProjectByIdAsync(projectId);
            if (project == null)
            {
                return Results.NotFound($"Проект с ID {projectId} не найден");
            }

            return Results.Ok(project);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при получении проекта {ProjectId}", projectId);
            return Results.Problem("Внутренняя ошибка сервера", statusCode: 500);
        }
    }

    private static async Task<IResult> UpdateProject(
        string projectId,
        [FromBody] UpdateProjectDto updateProjectDto,
        IProjectService projectService,
        ClaimsPrincipal user,
        ILogger<ProjectEndpoints> logger)
    {
        try
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Unauthorized();
            }

            var updatedProject = await projectService.UpdateProjectAsync(projectId, updateProjectDto, userId);
            if (updatedProject == null)
            {
                return Results.NotFound($"Проект с ID {projectId} не найден или нет прав доступа");
            }

            logger.LogInformation("Проект {ProjectId} обновлен пользователем {UserId}", projectId, userId);
            return Results.Ok(updatedProject);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при обновлении проекта {ProjectId}", projectId);
            return Results.Problem("Внутренняя ошибка сервера", statusCode: 500);
        }
    }

    private static async Task<IResult> DeleteProject(
        string projectId,
        IProjectService projectService,
        ClaimsPrincipal user,
        ILogger<ProjectEndpoints> logger)
    {
        try
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Unauthorized();
            }

            var deleted = await projectService.DeleteProjectAsync(projectId, userId);
            if (!deleted)
            {
                return Results.NotFound($"Проект с ID {projectId} не найден или нет прав доступа");
            }

            logger.LogInformation("Проект {ProjectId} удален пользователем {UserId}", projectId, userId);
            return Results.NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при удалении проекта {ProjectId}", projectId);
            return Results.Problem("Внутренняя ошибка сервера", statusCode: 500);
        }
    }

    private static async Task<IResult> AddMemberToProject(
        string projectId,
        string memberUserId,
        IProjectService projectService,
        ClaimsPrincipal user,
        ILogger<ProjectEndpoints> logger)
    {
        try
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Unauthorized();
            }

            var success = await projectService.AddMemberToProjectAsync(projectId, userId, memberUserId);
            if (!success)
            {
                return Results.NotFound($"Проект с ID {projectId} не найден или нет прав доступа");
            }

            logger.LogInformation("Пользователь {MemberUserId} добавлен в проект {ProjectId} пользователем {UserId}", 
                memberUserId, projectId, userId);
            
            return Results.Ok(new { message = "Участник успешно добавлен в проект" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при добавлении участника в проект {ProjectId}", projectId);
            return Results.Problem("Внутренняя ошибка сервера", statusCode: 500);
        }
    }

    private static async Task<IResult> RemoveMemberFromProject(
        string projectId,
        string memberUserId,
        IProjectService projectService,
        ClaimsPrincipal user,
        ILogger<ProjectEndpoints> logger)
    {
        try
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Unauthorized();
            }

            var success = await projectService.RemoveMemberFromProjectAsync(projectId, userId, memberUserId);
            if (!success)
            {
                return Results.NotFound($"Проект с ID {projectId} не найден или нет прав доступа");
            }

            logger.LogInformation("Пользователь {MemberUserId} удален из проекта {ProjectId} пользователем {UserId}", 
                memberUserId, projectId, userId);
            
            return Results.Ok(new { message = "Участник успешно удален из проекта" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при удалении участника из проекта {ProjectId}", projectId);
            return Results.Problem("Внутренняя ошибка сервера", statusCode: 500);
        }
    }
} 