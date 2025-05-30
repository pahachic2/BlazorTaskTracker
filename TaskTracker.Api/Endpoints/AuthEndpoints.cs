using TaskTracker.Api.Services;
using TaskTracker.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace TaskTracker.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var authGroup = app.MapGroup("/api/auth")
            .WithTags("Authentication")
            .WithOpenApi();

        // POST /api/auth/register - Регистрация нового пользователя
        authGroup.MapPost("/register", async (
            [FromBody] RegisterRequest request,
            [FromServices] IUserService userService) =>
        {
            // Валидация модели
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                var errors = validationResults.Select(vr => vr.ErrorMessage ?? "Ошибка валидации").ToList();
                return Results.BadRequest(new { success = false, message = "Ошибка валидации данных", errors });
            }

            var result = await userService.RegisterAsync(request);
            
            if (result != null)
            {
                return Results.Ok(result);
            }
            
            return Results.BadRequest(new { success = false, message = "Пользователь с таким именем или email уже существует" });
        })
        .WithName("RegisterUser")
        .WithSummary("Регистрация нового пользователя")
        .WithDescription("Создает нового пользователя в системе и возвращает JWT токен")
        .Produces<AuthResponse>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest);

        // POST /api/auth/login - Вход в систему
        authGroup.MapPost("/login", async (
            [FromBody] LoginRequest request,
            [FromServices] IUserService userService) =>
        {
            // Валидация модели
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                var errors = validationResults.Select(vr => vr.ErrorMessage ?? "Ошибка валидации").ToList();
                return Results.BadRequest(new { success = false, message = "Ошибка валидации данных", errors });
            }

            var result = await userService.LoginAsync(request);
            
            if (result != null)
            {
                return Results.Ok(result);
            }
            
            return Results.Unauthorized();
        })
        .WithName("LoginUser")
        .WithSummary("Вход в систему")
        .WithDescription("Аутентификация пользователя по email и паролю, возвращает JWT токен")
        .Produces<AuthResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);

        // GET /api/auth/me - Получение информации о текущем пользователе
        authGroup.MapGet("/me", async (
            HttpContext context,
            [FromServices] IUserService userService) =>
        {
            var usernameClaim = context.User.FindFirst("username");
            
            if (usernameClaim == null)
            {
                return Results.Unauthorized();
            }

            var user = await userService.GetUserByUsernameAsync(usernameClaim.Value);
            
            if (user != null)
            {
                return Results.Ok(new { 
                    username = user.Username, 
                    email = user.Email, 
                    createdAt = user.CreatedAt 
                });
            }
            
            return Results.NotFound();
        })
        .RequireAuthorization("RequireAuthenticatedUser")
        .WithName("GetCurrentUser")
        .WithSummary("Получение информации о текущем пользователе")
        .WithDescription("Возвращает информацию о пользователе по JWT токену")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);
    }
} 