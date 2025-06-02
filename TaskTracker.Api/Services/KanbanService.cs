using TaskTracker.Models;
using TaskTracker.Models.DTOs;

namespace TaskTracker.Api.Services;

public class KanbanService : IKanbanService
{
    private readonly IDatabaseService<KanbanColumn> _columnDatabase;
    private readonly IDatabaseService<KanbanTask> _taskDatabase;
    private readonly IDatabaseService<Project> _projectDatabase;
    private readonly IDatabaseService<UserProject> _userProjectDatabase;

    public KanbanService(
        IDatabaseService<KanbanColumn> columnDatabase,
        IDatabaseService<KanbanTask> taskDatabase,
        IDatabaseService<Project> projectDatabase,
        IDatabaseService<UserProject> userProjectDatabase)
    {
        _columnDatabase = columnDatabase;
        _taskDatabase = taskDatabase;
        _projectDatabase = projectDatabase;
        _userProjectDatabase = userProjectDatabase;
    }

    // Методы для колонок
    public async Task<List<ColumnResponse>> GetProjectColumnsAsync(string projectId, string userId)
    {
        // Проверяем доступ к проекту
        if (!await HasProjectAccessAsync(projectId, userId))
            return new List<ColumnResponse>();

        var columns = await _columnDatabase.FindAsync(c => c.ProjectId == projectId);
        var sortedColumns = columns.OrderBy(c => c.Order).ToList();

        var result = new List<ColumnResponse>();
        foreach (var column in sortedColumns)
        {
            var tasks = await _taskDatabase.FindAsync(t => t.ColumnId == column.Id);
            var sortedTasks = tasks.OrderBy(t => t.Order).ToList();

            result.Add(new ColumnResponse
            {
                Id = column.Id,
                Title = column.Title,
                ProjectId = column.ProjectId,
                Order = column.Order,
                CreatedAt = column.CreatedAt,
                UpdatedAt = column.UpdatedAt,
                Tasks = sortedTasks.Select(MapTaskToResponse).ToList()
            });
        }

        return result;
    }

    public async Task<ColumnResponse> CreateColumnAsync(CreateColumnRequest request, string userId)
    {
        // Проверяем доступ к проекту
        if (!await HasProjectAccessAsync(request.ProjectId, userId))
            throw new UnauthorizedAccessException("Нет доступа к проекту");

        var column = new KanbanColumn
        {
            Title = request.Title,
            ProjectId = request.ProjectId,
            Order = request.Order,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _columnDatabase.CreateAsync(column);

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

    public async Task<ColumnResponse?> UpdateColumnAsync(string columnId, UpdateColumnRequest request, string userId)
    {
        var column = await _columnDatabase.GetByIdAsync(columnId);
        if (column == null)
            return null;

        // Проверяем доступ к проекту
        if (!await HasProjectAccessAsync(column.ProjectId, userId))
            return null;

        column.Title = request.Title;
        column.Order = request.Order;
        column.UpdatedAt = DateTime.UtcNow;

        await _columnDatabase.UpdateAsync(columnId, column);

        var tasks = await _taskDatabase.FindAsync(t => t.ColumnId == columnId);
        
        return new ColumnResponse
        {
            Id = column.Id,
            Title = column.Title,
            ProjectId = column.ProjectId,
            Order = column.Order,
            CreatedAt = column.CreatedAt,
            UpdatedAt = column.UpdatedAt,
            Tasks = tasks.OrderBy(t => t.Order).Select(MapTaskToResponse).ToList()
        };
    }

    public async Task<bool> DeleteColumnAsync(string columnId, string userId)
    {
        var column = await _columnDatabase.GetByIdAsync(columnId);
        if (column == null)
            return false;

        // Проверяем доступ к проекту
        if (!await HasProjectAccessAsync(column.ProjectId, userId))
            return false;

        // Удаляем все задачи в колонке
        var tasks = await _taskDatabase.FindAsync(t => t.ColumnId == columnId);
        foreach (var task in tasks)
        {
            await _taskDatabase.DeleteAsync(task.Id);
        }

        await _columnDatabase.DeleteAsync(columnId);
        return true;
    }

    // Методы для задач
    public async Task<List<TaskResponse>> GetColumnTasksAsync(string columnId, string userId)
    {
        var column = await _columnDatabase.GetByIdAsync(columnId);
        if (column == null)
            return new List<TaskResponse>();

        // Проверяем доступ к проекту
        if (!await HasProjectAccessAsync(column.ProjectId, userId))
            return new List<TaskResponse>();

        var tasks = await _taskDatabase.FindAsync(t => t.ColumnId == columnId);
        return tasks.OrderBy(t => t.Order).Select(MapTaskToResponse).ToList();
    }

    public async Task<TaskResponse?> GetTaskByIdAsync(string taskId, string userId)
    {
        var task = await _taskDatabase.GetByIdAsync(taskId);
        if (task == null)
            return null;

        // Проверяем доступ к проекту
        if (!await HasProjectAccessAsync(task.ProjectId, userId))
            return null;

        return MapTaskToResponse(task);
    }

    public async Task<TaskResponse> CreateTaskAsync(CreateTaskRequest request, string userId)
    {
        var column = await _columnDatabase.GetByIdAsync(request.ColumnId);
        if (column == null)
            throw new ArgumentException("Колонка не найдена");

        // Проверяем доступ к проекту
        if (!await HasProjectAccessAsync(request.ProjectId, userId))
            throw new UnauthorizedAccessException("Нет доступа к проекту");

        var task = new KanbanTask
        {
            Title = request.Title,
            Description = request.Description ?? string.Empty,
            Tags = request.Tags,
            Assignees = request.Assignees,
            DueDate = request.DueDate,
            ColumnId = request.ColumnId,
            ProjectId = request.ProjectId,
            CreatedBy = userId,
            Priority = request.Priority,
            Status = Models.TaskStatus.ToDo,
            Order = await GetNextTaskOrderAsync(request.ColumnId),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _taskDatabase.CreateAsync(task);

        // Обновляем счетчик задач в проекте
        await UpdateProjectTaskCountAsync(request.ProjectId);

        return MapTaskToResponse(task);
    }

    public async Task<TaskResponse?> UpdateTaskAsync(string taskId, UpdateTaskRequest request, string userId)
    {
        var task = await _taskDatabase.GetByIdAsync(taskId);
        if (task == null)
            return null;

        // Проверяем доступ к проекту
        if (!await HasProjectAccessAsync(task.ProjectId, userId))
            return null;

        task.Title = request.Title;
        task.Description = request.Description ?? string.Empty;
        task.Tags = request.Tags;
        task.Assignees = request.Assignees;
        task.DueDate = request.DueDate;
        task.Priority = request.Priority;
        task.Status = request.Status;
        task.UpdatedAt = DateTime.UtcNow;

        await _taskDatabase.UpdateAsync(taskId, task);

        return MapTaskToResponse(task);
    }

    public async Task<bool> DeleteTaskAsync(string taskId, string userId)
    {
        var task = await _taskDatabase.GetByIdAsync(taskId);
        if (task == null)
            return false;

        // Проверяем доступ к проекту
        if (!await HasProjectAccessAsync(task.ProjectId, userId))
            return false;

        await _taskDatabase.DeleteAsync(taskId);

        // Обновляем счетчик задач в проекте
        await UpdateProjectTaskCountAsync(task.ProjectId);

        return true;
    }

    public async Task<TaskResponse?> MoveTaskAsync(string taskId, MoveTaskRequest request, string userId)
    {
        var task = await _taskDatabase.GetByIdAsync(taskId);
        if (task == null)
            return null;

        // Проверяем доступ к проекту
        if (!await HasProjectAccessAsync(task.ProjectId, userId))
            return null;

        // Проверяем, что новая колонка существует и принадлежит тому же проекту
        var newColumn = await _columnDatabase.GetByIdAsync(request.NewColumnId);
        if (newColumn == null || newColumn.ProjectId != task.ProjectId)
            return null;

        task.ColumnId = request.NewColumnId;
        task.Order = request.NewOrder;
        task.UpdatedAt = DateTime.UtcNow;

        // Обновляем статус задачи в зависимости от колонки
        task.Status = newColumn.Title.ToLower() switch
        {
            "to do" => Models.TaskStatus.ToDo,
            "in progress" => Models.TaskStatus.InProgress,
            "done" => Models.TaskStatus.Done,
            _ => task.Status
        };

        await _taskDatabase.UpdateAsync(taskId, task);

        return MapTaskToResponse(task);
    }

    // Вспомогательные методы
    private async Task<bool> HasProjectAccessAsync(string projectId, string userId)
    {
        var userProjects = await _userProjectDatabase.FindAsync(
            up => up.ProjectId == projectId && up.UserId == userId && up.IsActive);
        
        return userProjects.Any();
    }

    private async Task<int> GetNextTaskOrderAsync(string columnId)
    {
        var tasks = await _taskDatabase.FindAsync(t => t.ColumnId == columnId);
        return tasks.Any() ? tasks.Max(t => t.Order) + 1 : 0;
    }

    private async Task UpdateProjectTaskCountAsync(string projectId)
    {
        var tasks = await _taskDatabase.FindAsync(t => t.ProjectId == projectId);
        var project = await _projectDatabase.GetByIdAsync(projectId);
        
        if (project != null)
        {
            project.TaskCount = tasks.Count();
            project.UpdatedAt = DateTime.UtcNow;
            await _projectDatabase.UpdateAsync(projectId, project);
        }
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