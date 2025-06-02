using System.Text;
using Scalar.AspNetCore;
using TaskTracker.Api.Services;
using TaskTracker.Api.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using TaskTracker.Models.DTOs;
using TaskTracker.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Добавляем настройки базы данных
builder.Services.Configure<DatabaseSettings>(
    builder.Configuration.GetSection("Database"));

// Добавляем настройки JWT
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JWT"));

// Регистрируем сервисы
builder.Services.AddScoped(typeof(IDatabaseService<>), typeof(MongoDatabaseService<>));
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProjectService, ProjectService>();

// Настраиваем аутентификацию
var jwtSettings = builder.Configuration.GetSection("JWT").Get<JwtSettings>();
var key = Encoding.UTF8.GetBytes(jwtSettings!.SecretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Настраиваем авторизацию
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAuthenticatedUser", policy =>
        policy.RequireAuthenticatedUser());
});

builder.Services.AddSignalR();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Development", builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("*");
    });

    options.AddPolicy("Production", builder =>
    {
        builder.WithOrigins("https://tasktracker.graff.tech/");
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseCors("Development");
    app.MapOpenApi();
    app.MapScalarApiReference();
}
else
{
    app.UseCors("Production");
}

// Добавляем middleware для аутентификации и авторизации
app.UseAuthentication();
app.UseAuthorization();

// Регистрируем auth endpoints
app.MapAuthEndpoints();

// Регистрируем project endpoints
new ProjectEndpoints().MapEndpoints(app);

// Healthcheck endpoint
app.MapGet("/health", () => new { Status = "Healthy", Timestamp = DateTime.UtcNow })
    .WithName("HealthCheck")
    .WithSummary("Проверка состояния API")
    .WithOpenApi();

app.Run();
