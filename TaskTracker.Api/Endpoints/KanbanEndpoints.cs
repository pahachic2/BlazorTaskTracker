using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;
using TaskTracker.Api.Services; // For IKanbanService

namespace TaskTracker.Api.Endpoints
{
    public class KanbanEndpoints : IEndpointGroup
    {
        public void MapEndpoints(WebApplication app)
        {
            var group = app.MapGroup("/api")
                           .RequireAuthorization(); // Apply authorization to all endpoints in this group

            group.MapGet("/projects", async (HttpContext httpContext, IKanbanService kanbanService) =>
            {
                var userId = GetUserId(httpContext);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var projects = await kanbanService.GetProjectsByUserIdAsync(userId);
                return Results.Ok(projects);
            })
            .WithName("GetUserProjects")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/projects/{projectId}/board", async (HttpContext httpContext, string projectId, IKanbanService kanbanService) =>
            {
                var userId = GetUserId(httpContext);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var board = await kanbanService.GetBoardByProjectIdAsync(projectId, userId);
                if (board == null)
                {
                    return Results.NotFound("Board not found or user does not have access.");
                }
                return Results.Ok(board);
            })
            .WithName("GetProjectBoard")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);
        }

        private string? GetUserId(HttpContext httpContext)
        {
            // Assuming the User ID is stored in the 'NameIdentifier' claim, which is common.
            // Adjust if a different claim type is used for User ID.
            return httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
