using TaskTracker.Models;
using TaskTracker.Models.DTOs;

namespace TaskTracker.Api.Services;

public interface IKanbanService
{
    // Методы для колонок
    Task<List<ColumnResponse>> GetProjectColumnsAsync(string projectId, string userId);
    Task<ColumnResponse> CreateColumnAsync(CreateColumnRequest request, string userId);
    Task<ColumnResponse?> UpdateColumnAsync(string columnId, UpdateColumnRequest request, string userId);
    Task<bool> DeleteColumnAsync(string columnId, string userId);

    // Методы для задач
    Task<List<TaskResponse>> GetColumnTasksAsync(string columnId, string userId);
    Task<TaskResponse?> GetTaskByIdAsync(string taskId, string userId);
    Task<TaskResponse> CreateTaskAsync(CreateTaskRequest request, string userId);
    Task<TaskResponse?> UpdateTaskAsync(string taskId, UpdateTaskRequest request, string userId);
    Task<bool> DeleteTaskAsync(string taskId, string userId);
    Task<TaskResponse?> MoveTaskAsync(string taskId, MoveTaskRequest request, string userId);
} 