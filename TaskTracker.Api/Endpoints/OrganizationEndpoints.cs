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

        // Существующие endpoints
        group.MapGet("/", GetUserOrganizations)
            .WithName("GetUserOrganizations")
            .WithSummary("Получить организации пользователя")
            .WithOpenApi();

        group.MapGet("/{organizationId}", GetOrganizationById)
            .WithName("GetOrganizationById")
            .WithSummary("Получить организацию по ID")
            .WithOpenApi();

        group.MapPost("/", CreateOrganization)
            .WithName("CreateOrganization")
            .WithSummary("Создать новую организацию")
            .WithOpenApi();

        group.MapPut("/{organizationId}", UpdateOrganization)
            .WithName("UpdateOrganization")
            .WithSummary("Обновить организацию")
            .WithOpenApi();

        group.MapDelete("/{organizationId}", DeleteOrganization)
            .WithName("DeleteOrganization")
            .WithSummary("Удалить организацию")
            .WithOpenApi();

        group.MapPost("/{organizationId}/members", AddMemberToOrganization)
            .WithName("AddMemberToOrganization")
            .WithSummary("Добавить участника к организации")
            .WithOpenApi();

        group.MapDelete("/{organizationId}/members/{memberUserId}", RemoveMemberFromOrganization)
            .WithName("RemoveMemberFromOrganization")
            .WithSummary("Удалить участника из организации")
            .WithOpenApi();

        group.MapGet("/admin/all", GetAllOrganizations)
            .WithName("GetAllOrganizations")
            .WithSummary("Получить все организации (админ)")
            .WithOpenApi();

        // НОВЫЕ ENDPOINTS ДЛЯ СИСТЕМЫ ПРИГЛАШЕНИЙ

        // Получить участников организации
        group.MapGet("/{organizationId}/members", GetOrganizationMembers)
            .WithName("GetOrganizationMembers")
            .WithSummary("Получить список участников организации")
            .WithOpenApi();

        // Пригласить пользователя в организацию
        group.MapPost("/{organizationId}/invitations", InviteUser)
            .WithName("InviteUserToOrganization")
            .WithSummary("Пригласить пользователя в организацию")
            .WithOpenApi();

        // Получить список приглашений организации
        group.MapGet("/{organizationId}/invitations", GetOrganizationInvitations)
            .WithName("GetOrganizationInvitations")
            .WithSummary("Получить список приглашений организации")
            .WithOpenApi();

        // Отозвать приглашение
        group.MapDelete("/invitations/{invitationId}", RevokeInvitation)
            .WithName("RevokeInvitation")
            .WithSummary("Отозвать приглашение")
            .WithOpenApi();

        // ПУБЛИЧНЫЕ ENDPOINTS ДЛЯ ОБРАБОТКИ ПРИГЛАШЕНИЙ (без авторизации)
        var publicGroup = endpoints.MapGroup("/api/invitations")
            .WithTags("Invitations");

        // Получить информацию о приглашении по токену
        publicGroup.MapGet("/{token}", GetInvitationInfo)
            .WithName("GetInvitationInfo")
            .WithSummary("Получить информацию о приглашении")
            .WithOpenApi();

        // Принять приглашение
        publicGroup.MapPost("/{token}/accept", AcceptInvitation)
            .WithName("AcceptInvitation")
            .WithSummary("Принять приглашение в организацию")
            .WithOpenApi();

        // Отклонить приглашение
        publicGroup.MapPost("/{token}/decline", DeclineInvitation)
            .WithName("DeclineInvitation")
            .WithSummary("Отклонить приглашение в организацию")
            .WithOpenApi();
    }

    // СУЩЕСТВУЮЩИЕ МЕТОДЫ
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

    // НОВЫЕ МЕТОДЫ ДЛЯ СИСТЕМЫ ПРИГЛАШЕНИЙ

    private static async Task<IResult> GetOrganizationMembers(
        string organizationId,
        IOrganizationService organizationService,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var members = await organizationService.GetOrganizationMembersAsync(organizationId, userId);
        return Results.Ok(members);
    }

    private static async Task<IResult> InviteUser(
        string organizationId,
        InviteUserRequest request,
        IOrganizationService organizationService,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        try
        {
            var invitation = await organizationService.InviteUserAsync(organizationId, request, userId);
            return Results.Created($"/api/invitations/{invitation.Id}", invitation);
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Forbid();
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Results.Conflict(ex.Message);
        }
    }

    private static async Task<IResult> GetOrganizationInvitations(
        string organizationId,
        IOrganizationService organizationService,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var invitations = await organizationService.GetOrganizationInvitationsAsync(organizationId, userId);
        return Results.Ok(invitations);
    }

    private static async Task<IResult> RevokeInvitation(
        string invitationId,
        IOrganizationService organizationService,
        ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var success = await organizationService.RevokeInvitationAsync(invitationId, userId);
        if (!success)
            return Results.NotFound();

        return Results.NoContent();
    }

    // ПУБЛИЧНЫЕ МЕТОДЫ (без авторизации)
    private static async Task<IResult> GetInvitationInfo(
        string token,
        IOrganizationService organizationService)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Results.BadRequest("Токен приглашения обязателен");

        var invitationInfo = await organizationService.GetInvitationInfoAsync(token);
        return Results.Ok(invitationInfo);
    }

    private static async Task<IResult> AcceptInvitation(
        string token,
        AcceptInvitationRequest request,
        IOrganizationService organizationService)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Results.BadRequest("Токен приглашения обязателен");

        // Токен берем из URL параметра, а не из body
        request.Token = token;

        try
        {
            var result = await organizationService.AcceptInvitationAsync(request);
            return Results.Ok(result);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }

    private static async Task<IResult> DeclineInvitation(
        string token,
        IOrganizationService organizationService)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Results.BadRequest("Токен приглашения обязателен");

        var request = new DeclineInvitationRequest { Token = token };
        var success = await organizationService.DeclineInvitationAsync(request);
        
        if (!success)
            return Results.BadRequest("Не удалось отклонить приглашение");

        return Results.Ok(new { Message = "Приглашение успешно отклонено" });
    }
}

/// <summary>
/// DTO для добавления участника к организации
/// </summary>
public class AddOrganizationMemberRequest
{
    public string UserId { get; set; } = string.Empty;
} 