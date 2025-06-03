using System.Security.Claims;
using TaskTracker.Api.Services;
using TaskTracker.Models;
using TaskTracker.Models.DTOs;

namespace TaskTracker.Api.Endpoints;

public static class OrganizationEndpoints
{
    public static void MapOrganizationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/organizations")
            .WithTags("Organizations")
            .RequireAuthorization("RequireAuthenticatedUser");

        // Получить организации пользователя
        group.MapGet("/", GetUserOrganizations)
            .WithName("GetUserOrganizations")
            .WithSummary("Получить организации пользователя")
            .WithOpenApi();

        // Получить организацию по ID
        group.MapGet("/{organizationId}", GetOrganizationById)
            .WithName("GetOrganizationById")
            .WithSummary("Получить организацию по ID")
            .WithOpenApi();

        // Создать новую организацию
        group.MapPost("/", CreateOrganization)
            .WithName("CreateOrganization")
            .WithSummary("Создать новую организацию")
            .WithOpenApi();

        // Обновить организацию
        group.MapPut("/{organizationId}", UpdateOrganization)
            .WithName("UpdateOrganization")
            .WithSummary("Обновить организацию")
            .WithOpenApi();

        // Удалить организацию
        group.MapDelete("/{organizationId}", DeleteOrganization)
            .WithName("DeleteOrganization")
            .WithSummary("Удалить организацию")
            .WithOpenApi();

        // Добавить участника к организации
        group.MapPost("/{organizationId}/members", AddMemberToOrganization)
            .WithName("AddMemberToOrganization")
            .WithSummary("Добавить участника к организации")
            .WithOpenApi();

        // Удалить участника из организации
        group.MapDelete("/{organizationId}/members/{memberUserId}", RemoveMemberFromOrganization)
            .WithName("RemoveMemberFromOrganization")
            .WithSummary("Удалить участника из организации")
            .WithOpenApi();

        // Endpoint для получения всех организаций (только для админов)
        group.MapGet("/admin/all", GetAllOrganizations)
            .WithName("GetAllOrganizations")
            .WithSummary("Получить все организации (админ)")
            .WithOpenApi();
    }

    private static async Task<IResult> GetUserOrganizations(
        IOrganizationService organizationService,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var organizations = await organizationService.GetUserOrganizationsAsync(userId);
        return Results.Ok(organizations);
    }

    private static async Task<IResult> GetOrganizationById(
        string organizationId,
        IOrganizationService organizationService,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var organization = await organizationService.GetOrganizationByIdAsync(organizationId, userId);
        if (organization == null)
            return Results.NotFound();

        return Results.Ok(organization);
    }

    private static async Task<IResult> CreateOrganization(
        CreateOrganizationRequest request,
        IOrganizationService organizationService,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var organization = await organizationService.CreateOrganizationAsync(request, userId);
        return Results.Created($"/api/organizations/{organization.Id}", organization);
    }

    private static async Task<IResult> UpdateOrganization(
        string organizationId,
        UpdateOrganizationRequest request,
        IOrganizationService organizationService,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var organization = await organizationService.UpdateOrganizationAsync(organizationId, request, userId);
        if (organization == null)
            return Results.NotFound();

        return Results.Ok(organization);
    }

    private static async Task<IResult> DeleteOrganization(
        string organizationId,
        IOrganizationService organizationService,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var success = await organizationService.DeleteOrganizationAsync(organizationId, userId);
        if (!success)
            return Results.NotFound();

        return Results.NoContent();
    }

    private static async Task<IResult> AddMemberToOrganization(
        string organizationId,
        AddOrganizationMemberRequest request,
        IOrganizationService organizationService,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var success = await organizationService.AddMemberToOrganizationAsync(
            organizationId, userId, request.UserId);

        if (!success)
            return Results.BadRequest("Не удалось добавить участника");

        return Results.Ok();
    }

    private static async Task<IResult> RemoveMemberFromOrganization(
        string organizationId,
        string memberUserId,
        IOrganizationService organizationService,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var success = await organizationService.RemoveMemberFromOrganizationAsync(
            organizationId, userId, memberUserId);

        if (!success)
            return Results.BadRequest("Не удалось удалить участника");

        return Results.Ok();
    }

    private static async Task<IResult> GetAllOrganizations(
        IOrganizationService organizationService,
        ClaimsPrincipal user)
    {
        // TODO: Добавить проверку роли админа
        var organizations = await organizationService.GetAllOrganizationsAsync();
        return Results.Ok(organizations);
    }
}

/// <summary>
/// DTO для добавления участника к организации
/// </summary>
public class AddOrganizationMemberRequest
{
    public string UserId { get; set; } = string.Empty;
} 