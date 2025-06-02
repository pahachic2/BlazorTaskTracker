using System.Security.Claims;
using TaskTracker.Api.Services;
using TaskTracker.Models.DTOs;

namespace TaskTracker.Api.Endpoints;

public static class ColumnEndpoints
{
    public static void MapColumnEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/columns")
            .WithTags("Columns")
            .RequireAuthorization("RequireAuthenticatedUser");

        // Получить колонки проекта
        group.MapGet("/project/{projectId}", GetProjectColumns)
            .WithName("GetProjectColumnsOnly")
            .WithSummary("Получить колонки проекта")
            .WithOpenApi();

        // Получить колонку по ID
        group.MapGet("/{columnId}", GetColumnById)
            .WithName("GetColumnByIdOnly")
            .WithSummary("Получить колонку по ID")
            .WithOpenApi();

        // Создать новую колонку
        group.MapPost("/", CreateColumn)
            .WithName("CreateColumnOnly")
            .WithSummary("Создать новую колонку")
            .WithOpenApi();

        // Обновить колонку
        group.MapPut("/{columnId}", UpdateColumn)
            .WithName("UpdateColumnOnly")
            .WithSummary("Обновить колонку")
            .WithOpenApi();

        // Удалить колонку
        group.MapDelete("/{columnId}", DeleteColumn)
            .WithName("DeleteColumnOnly")
            .WithSummary("Удалить колонку")
            .WithOpenApi();

        // Изменить порядок колонок
        group.MapPut("/project/{projectId}/reorder", ReorderColumns)
            .WithName("ReorderColumnsOnly")
            .WithSummary("Изменить порядок колонок")
            .WithOpenApi();
    }

    private static async Task<IResult> GetProjectColumns(
        string projectId,
        IColumnService columnService,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var columns = await columnService.GetProjectColumnsAsync(projectId, userId);
        return Results.Ok(columns);
    }

    private static async Task<IResult> GetColumnById(
        string columnId,
        IColumnService columnService,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var column = await columnService.GetColumnByIdAsync(columnId, userId);
        if (column == null)
            return Results.NotFound();

        return Results.Ok(column);
    }

    private static async Task<IResult> CreateColumn(
        CreateColumnRequest request,
        IColumnService columnService,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        try
        {
            var column = await columnService.CreateColumnAsync(request, userId);
            return Results.Created($"/api/columns/{column.Id}", column);
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Forbid();
        }
    }

    private static async Task<IResult> UpdateColumn(
        string columnId,
        UpdateColumnRequest request,
        IColumnService columnService,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var column = await columnService.UpdateColumnAsync(columnId, request, userId);
        if (column == null)
            return Results.NotFound();

        return Results.Ok(column);
    }

    private static async Task<IResult> DeleteColumn(
        string columnId,
        IColumnService columnService,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        try
        {
            var success = await columnService.DeleteColumnAsync(columnId, userId);
            if (!success)
                return Results.NotFound();

            return Results.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { Message = ex.Message });
        }
    }

    private static async Task<IResult> ReorderColumns(
        string projectId,
        ReorderColumnsRequest request,
        IColumnService columnService,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var success = await columnService.ReorderColumnsAsync(projectId, request, userId);
        if (!success)
            return Results.Forbid();

        return Results.Ok();
    }
} 