using TaskTracker.Api.Services;
using TaskTracker.Models.DTOs;

namespace TaskTracker.Api.Endpoints;

public class AuthEndpoints : IEndpointGroup
{
    public void MapEndpoints(WebApplication app)
    {
        var authGroup = app.MapGroup("/api/auth")
            .WithTags("Authentication")
            .WithOpenApi();

        authGroup.MapPost("/register", RegisterAsync)
            .WithName("Register")
            .WithSummary("Регистрация нового пользователя")
            .WithDescription("Создает новый аккаунт пользователя и возвращает JWT токен")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest);

        authGroup.MapPost("/login", LoginAsync)
            .WithName("Login")
            .WithSummary("Вход в систему")
            .WithDescription("Аутентификация пользователя и получение JWT токена")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private async Task<IResult> RegisterAsync(RegisterRequest request, IUserService userService)
    {
        try
        {
            var result = await userService.RegisterAsync(request);
            
            if (result == null)
            {
                return Results.BadRequest(new { 
                    message = "Пользователь с таким именем или email уже существует",
                    error = "USER_ALREADY_EXISTS"
                });
            }
            
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { 
                message = "Ошибка при регистрации пользователя",
                error = "REGISTRATION_ERROR",
                details = ex.Message
            });
        }
    }

    private async Task<IResult> LoginAsync(LoginRequest request, IUserService userService)
    {
        try
        {
            var result = await userService.LoginAsync(request);
            
            if (result == null)
            {
                return Results.Unauthorized();
            }
            
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { 
                message = "Ошибка при входе в систему",
                error = "LOGIN_ERROR",
                details = ex.Message
            });
        }
    }
} 