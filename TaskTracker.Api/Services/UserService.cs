using TaskTracker.Models;
using TaskTracker.Models.DTOs;

namespace TaskTracker.Api.Services;

public class UserService : IUserService
{
    private readonly IDatabaseService<User> _databaseService;
    private readonly IJwtService _jwtService;

    public UserService(IDatabaseService<User> databaseService, IJwtService jwtService)
    {
        _databaseService = databaseService;
        _jwtService = jwtService;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        // Проверяем, существует ли пользователь
        if (await UserExistsAsync(request.Username, request.Email))
        {
            return null;
        }

        // Создаем нового пользователя
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var createdUser = await _databaseService.CreateAsync(user);
        
        // Генерируем токен
        var token = _jwtService.GenerateToken(createdUser.Id, createdUser.Username);
        
        return new AuthResponse
        {
            Token = token,
            Username = createdUser.Username,
            Email = createdUser.Email,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30) // Из настроек JWT
        };
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = await GetUserByEmailAsync(request.Email);
        
        if (user == null || !user.IsActive)
        {
            return null;
        }

        // Проверяем пароль
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        // Генерируем токен
        var token = _jwtService.GenerateToken(user.Id, user.Username);
        
        return new AuthResponse
        {
            Token = token,
            Username = user.Username,
            Email = user.Email,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30) // Из настроек JWT
        };
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        var users = await _databaseService.FindAsync(u => u.Email == email);
        return users.FirstOrDefault();
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        var users = await _databaseService.FindAsync(u => u.Username == username);
        return users.FirstOrDefault();
    }

    public async Task<bool> UserExistsAsync(string username, string email)
    {
        var users = await _databaseService.FindAsync(u => u.Username == username || u.Email == email);
        return users.Any();
    }
} 