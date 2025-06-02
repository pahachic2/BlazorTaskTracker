using TaskTracker.Models;
using TaskTracker.Models.DTOs;

namespace TaskTracker.Api.Services;

public class ColumnService : IColumnService
{
    private readonly IDatabaseService<KanbanColumn> _columnDatabase;
    private readonly IDatabaseService<KanbanTask> _taskDatabase;
    private readonly IDatabaseService<UserProject> _userProjectDatabase;

    public ColumnService(
        IDatabaseService<KanbanColumn> columnDatabase,
        IDatabaseService<KanbanTask> taskDatabase,
        IDatabaseService<UserProject> userProjectDatabase)
    {
        _columnDatabase = columnDatabase;
        _taskDatabase = taskDatabase;
        _userProjectDatabase = userProjectDatabase;
    }

    public async Task<List<ColumnResponse>> GetProjectColumnsAsync(string projectId, string userId)
    {
        // Проверяем доступ пользователя к проекту
        if (!await HasProjectAccess(projectId, userId))
            return new List<ColumnResponse>();

        // Получаем колонки проекта, отсортированные по порядку
        var columns = await _columnDatabase.FindAsync(
            c => c.ProjectId == projectId);

        var columnResponses = new List<ColumnResponse>();

        foreach (var column in columns.OrderBy(c => c.Order))
        {
            // Получаем задачи для каждой колонки
            var tasks = await _taskDatabase.FindAsync(
                t => t.ColumnId == column.Id);

            var columnResponse = MapToResponse(column);
            columnResponse.Tasks = tasks.OrderBy(t => t.Order)
                .Select(MapTaskToResponse).ToList();

            columnResponses.Add(columnResponse);
        }

        return columnResponses;
    }

    public async Task<ColumnResponse?> GetColumnByIdAsync(string columnId, string userId)
    {
        var column = await _columnDatabase.GetByIdAsync(columnId);
        if (column == null)
            return null;

        // Проверяем доступ пользователя к проекту
        if (!await HasProjectAccess(column.ProjectId, userId))
            return null;

        // Получаем задачи колонки
        var tasks = await _taskDatabase.FindAsync(t => t.ColumnId == columnId);
        
        var columnResponse = MapToResponse(column);
        columnResponse.Tasks = tasks.OrderBy(t => t.Order)
            .Select(MapTaskToResponse).ToList();

        return columnResponse;
    }

    public async Task<ColumnResponse> CreateColumnAsync(CreateColumnRequest request, string userId)
    {
        // Проверяем доступ пользователя к проекту
        if (!await HasProjectAccess(request.ProjectId, userId))
            throw new UnauthorizedAccessException("Нет доступа к проекту");

        // Если порядок не указан, ставим в конец
        if (request.Order == 0)
        {
            var existingColumns = await _columnDatabase.FindAsync(
                c => c.ProjectId == request.ProjectId);
            request.Order = existingColumns.Count() + 1;
        }

        var column = new KanbanColumn
        {
            Title = request.Title,
            ProjectId = request.ProjectId,
            Order = request.Order,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _columnDatabase.CreateAsync(column);
        return MapToResponse(column);
    }

    public async Task<ColumnResponse?> UpdateColumnAsync(string columnId, UpdateColumnRequest request, string userId)
    {
        var column = await _columnDatabase.GetByIdAsync(columnId);
        if (column == null)
            return null;

        // Проверяем доступ пользователя к проекту
        if (!await HasProjectAccess(column.ProjectId, userId))
            return null;

        column.Title = request.Title;
        column.Order = request.Order;
        column.UpdatedAt = DateTime.UtcNow;

        await _columnDatabase.UpdateAsync(columnId, column);
        return MapToResponse(column);
    }

    public async Task<bool> DeleteColumnAsync(string columnId, string userId)
    {
        var column = await _columnDatabase.GetByIdAsync(columnId);
        if (column == null)
            return false;

        // Проверяем доступ пользователя к проекту
        if (!await HasProjectAccess(column.ProjectId, userId))
            return false;

        // Проверяем, есть ли задачи в колонке
        var tasks = await _taskDatabase.FindAsync(t => t.ColumnId == columnId);
        if (tasks.Any())
            throw new InvalidOperationException("Нельзя удалить колонку с задачами");

        await _columnDatabase.DeleteAsync(columnId);
        return true;
    }

    public async Task<bool> ReorderColumnsAsync(string projectId, ReorderColumnsRequest request, string userId)
    {
        // Проверяем доступ пользователя к проекту
        if (!await HasProjectAccess(projectId, userId))
            return false;

        // Обновляем порядок колонок
        foreach (var columnOrder in request.Columns)
        {
            var column = await _columnDatabase.GetByIdAsync(columnOrder.Id);
            if (column != null && column.ProjectId == projectId)
            {
                column.Order = columnOrder.Order;
                column.UpdatedAt = DateTime.UtcNow;
                await _columnDatabase.UpdateAsync(columnOrder.Id, column);
            }
        }

        return true;
    }

    private async Task<bool> HasProjectAccess(string projectId, string userId)
    {
        var userProjects = await _userProjectDatabase.FindAsync(
            up => up.ProjectId == projectId && up.UserId == userId && up.IsActive);
        
        return userProjects.Any();
    }

    private static ColumnResponse MapToResponse(KanbanColumn column)
    {
        return new ColumnResponse
        {
            Id = column.Id,
            Title = column.Title,
            ProjectId = column.ProjectId,
            Order = column.Order,
            CreatedAt = column.CreatedAt,
            UpdatedAt = column.UpdatedAt,
            Tasks = new List<TaskResponse>()
        };
    }

    private static TaskResponse MapTaskToResponse(KanbanTask task)
    {
        return new TaskResponse
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Tags = task.Tags,
            Assignees = task.Assignees,
            DueDate = task.DueDate,
            ColumnId = task.ColumnId,
            ProjectId = task.ProjectId,
            CreatedBy = task.CreatedBy,
            Order = task.Order,
            Priority = task.Priority,
            Status = task.Status,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        };
    }
} 