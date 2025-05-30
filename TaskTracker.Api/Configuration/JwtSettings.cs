namespace TaskTracker.Api.Configuration;

public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public int Expiration { get; set; }
} 