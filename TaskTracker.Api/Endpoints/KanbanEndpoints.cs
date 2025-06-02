using System.Security.Claims;
using TaskTracker.Api.Services;
using TaskTracker.Models.DTOs;

namespace TaskTracker.Api.Endpoints;

public static class KanbanEndpoints
{
    public static void MapKanbanEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/kanban")
            .WithTags("Kanban")
            .RequireAuthorization("RequireAuthenticatedUser");

        // Эндпоинты для колонок
        group.MapGet("/projects/{projectId}/columns", GetProjectColumns)
            .WithName("GetProjectColumns")
            .WithSummary("Получить колонки проекта с задачами")
            .WithOpenApi();

        group.MapPost("/columns", CreateColumn)
            .WithName("CreateColumn")
            .WithSummary("Создать новую колонку")
            .WithOpenApi();

        group.MapPut("/columns/{columnId}", UpdateColumn)
            .WithName("UpdateColumn")
            .WithSummary("Обновить колонку")
            .WithOpenApi();

        group.MapDelete("/columns/{columnId}", DeleteColumn)
            .WithName("DeleteColumn")
            .WithSummary("Удалить колонку")
            .WithOpenApi();

        // Эндпоинты для задач
        group.MapGet("/columns/{columnId}/tasks", GetColumnTasks)
            .WithName("GetColumnTasks")
            .WithSummary("Получить задачи колонки")
            .WithOpenApi();

        group.MapGet("/tasks/{taskId}", GetTaskById)
            .WithName("GetTaskById")
            .WithSummary("Получить задачу по ID")
            .WithOpenApi();

        group.MapPost("/tasks", CreateTask)
            .WithName("CreateTask")
            .WithSummary("Создать новую задачу")
            .WithOpenApi();

        group.MapPut("/tasks/{taskId}", UpdateTask)
            .WithName("UpdateTask")
            .WithSummary("Обновить задачу")
            .WithOpenApi();

        group.MapDelete("/tasks/{taskId}", DeleteTask)
            .WithName("DeleteTask")
            .WithSummary("Удалить задачу")
            .WithOpenApi();

        group.MapPut("/tasks/{taskId}/move", MoveTask)
            .WithName("MoveTask")
            .WithSummary("Переместить задачу")
            .WithOpenApi();
    }

    // Методы для колонок
    private static async Task<IResult> GetProjectColumns(
        string projectId,
        IKanbanService kanbanService,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var columns = await kanbanService.GetProjectColumnsAsync(projectId, userId);
        return Results.Ok(columns);
    }

    private static async Task<IResult> CreateColumn(
        CreateColumnRequest request,
        IKanbanService kanbanService,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        try
        {
            var column = await kanbanService.CreateColumnAsync(request, userId);
            return Results.Created($"/api/kanban/columns/{column.Id}", column);
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Forbid();
        }
    }

    private static async Task<IResult> UpdateColumn(
        string columnId,
        UpdateColumnRequest request,
        IKanbanService kanbanService,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var column = await kanbanService.UpdateColumnAsync(columnId, request, userId);
        if (column == null)
            return Results.NotFound();

        return Results.Ok(column);
    }

    private static async Task<IResult> DeleteColumn(
        string columnId,
        IKanbanService kanbanService,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var success = await kanbanService.DeleteColumnAsync(columnId, userId);
        if (!success)
            return Results.NotFound();

        return Results.NoContent();
    }

    // Методы для задач
    private static async Task<IResult> GetColumnTasks(
        string columnId,
        IKanbanService kanbanService,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var tasks = await kanbanService.GetColumnTasksAsync(columnId, userId);
        return Results.Ok(tasks);
    }

    private static async Task<IResult> GetTaskById(
        string taskId,
        IKanbanService kanbanService,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var task = await kanbanService.GetTaskByIdAsync(taskId, userId);
        if (task == null)
            return Results.NotFound();

        return Results.Ok(task);
    }

    private static async Task<IResult> CreateTask(
        CreateTaskRequest request,
        IKanbanService kanbanService,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        try
        {
            var task = await kanbanService.CreateTaskAsync(request, userId);
            return Results.Created($"/api/kanban/tasks/{task.Id}", task);
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Forbid();
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }

    private static async Task<IResult> UpdateTask(
        string taskId,
        UpdateTaskRequest request,
        IKanbanService kanbanService,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var task = await kanbanService.UpdateTaskAsync(taskId, request, userId);
        if (task == null)
            return Results.NotFound();

        return Results.Ok(task);
    }

    private static async Task<IResult> DeleteTask(
        string taskId,
        IKanbanService kanbanService,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var success = await kanbanService.DeleteTaskAsync(taskId, userId);
        if (!success)
            return Results.NotFound();

        return Results.NoContent();
    }

    private static async Task<IResult> MoveTask(
        string taskId,
        MoveTaskRequest request,
        IKanbanService kanbanService,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var task = await kanbanService.MoveTaskAsync(taskId, request, userId);
        if (task == null)
            return Results.NotFound();

        return Results.Ok(task);
    }
} 