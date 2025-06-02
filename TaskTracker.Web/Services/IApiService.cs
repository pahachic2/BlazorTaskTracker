using TaskTracker.Models.DTOs;

namespace TaskTracker.Web.Services;

public interface IApiService
{
    // Методы аутентификации
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
    
    // Методы для проектов
    Task<List<ProjectResponse>> GetUserProjectsAsync();
    Task<ProjectResponse?> GetProjectByIdAsync(string projectId);
    Task<ProjectResponse?> CreateProjectAsync(CreateProjectRequest request);
    Task<ProjectResponse?> UpdateProjectAsync(string projectId, UpdateProjectRequest request);
    Task<bool> DeleteProjectAsync(string projectId);
    
    // Методы для канбан доски
    Task<List<ColumnResponse>> GetProjectColumnsAsync(string projectId);
    Task<ColumnResponse?> CreateColumnAsync(CreateColumnRequest request);
    Task<ColumnResponse?> UpdateColumnAsync(string columnId, UpdateColumnRequest request);
    Task<bool> DeleteColumnAsync(string columnId);
    
    // Методы для задач
    Task<List<TaskResponse>> GetColumnTasksAsync(string columnId);
    Task<TaskResponse?> GetTaskByIdAsync(string taskId);
    Task<TaskResponse?> CreateTaskAsync(CreateTaskRequest request);
    Task<TaskResponse?> UpdateTaskAsync(string taskId, UpdateTaskRequest request);
    Task<bool> DeleteTaskAsync(string taskId);
    Task<TaskResponse?> MoveTaskAsync(string taskId, MoveTaskRequest request);
} 