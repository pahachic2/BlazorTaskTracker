using TaskTracker.Models.DTOs;

namespace TaskTracker.Api.Services;

public interface IColumnService
{
    Task<List<ColumnResponse>> GetProjectColumnsAsync(string projectId, string userId);
    Task<ColumnResponse?> GetColumnByIdAsync(string columnId, string userId);
    Task<ColumnResponse> CreateColumnAsync(CreateColumnRequest request, string userId);
    Task<ColumnResponse?> UpdateColumnAsync(string columnId, UpdateColumnRequest request, string userId);
    Task<bool> DeleteColumnAsync(string columnId, string userId);
    Task<bool> ReorderColumnsAsync(string projectId, ReorderColumnsRequest request, string userId);
} 