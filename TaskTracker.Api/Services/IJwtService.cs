namespace TaskTracker.Api.Services;

public interface IJwtService
{
    string GenerateToken(string userId, string username);
    bool ValidateToken(string token);
    string? GetUserIdFromToken(string token);
} 