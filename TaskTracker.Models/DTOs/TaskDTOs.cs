using System.ComponentModel.DataAnnotations;

namespace TaskTracker.Models.DTOs;

/// <summary>
/// DTO для создания задачи
/// </summary>
public class CreateTaskRequest
{
    [Required(ErrorMessage = "Название задачи обязательно")]
    [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов")]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(1000, ErrorMessage = "Описание не должно превышать 1000 символов")]
    public string? Description { get; set; }
    
    public List<string> Tags { get; set; } = new();
    public List<string> Assignees { get; set; } = new();
    public DateTime? DueDate { get; set; }
    
    [Required(ErrorMessage = "ID колонки обязательно")]
    public string ColumnId { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "ID проекта обязательно")]
    public string ProjectId { get; set; } = string.Empty;
    
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
}

/// <summary>
/// DTO для обновления задачи
/// </summary>
public class UpdateTaskRequest
{
    [Required(ErrorMessage = "Название задачи обязательно")]
    [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов")]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(1000, ErrorMessage = "Описание не должно превышать 1000 символов")]
    public string? Description { get; set; }
    
    public List<string> Tags { get; set; } = new();
    public List<string> Assignees { get; set; } = new();
    public DateTime? DueDate { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public TaskStatus Status { get; set; } = TaskStatus.ToDo;
}

/// <summary>
/// DTO для перемещения задачи
/// </summary>
public class MoveTaskRequest
{
    [Required(ErrorMessage = "ID новой колонки обязательно")]
    public string NewColumnId { get; set; } = string.Empty;
    
    public int NewOrder { get; set; } = 0;
}

/// <summary>
/// DTO для ответа с данными задачи
/// </summary>
public class TaskResponse
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public List<string> Assignees { get; set; } = new();
    public DateTime? DueDate { get; set; }
    public string ColumnId { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public int Order { get; set; }
    public TaskPriority Priority { get; set; }
    public TaskStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO для создания колонки
/// </summary>
public class CreateColumnRequest
{
    [Required(ErrorMessage = "Название колонки обязательно")]
    [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
    public string Title { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "ID проекта обязательно")]
    public string ProjectId { get; set; } = string.Empty;
    
    public int Order { get; set; } = 0;
}

/// <summary>
/// DTO для обновления колонки
/// </summary>
public class UpdateColumnRequest
{
    [Required(ErrorMessage = "Название колонки обязательно")]
    [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
    public string Title { get; set; } = string.Empty;
    
    public int Order { get; set; } = 0;
}

/// <summary>
/// DTO для ответа с данными колонки
/// </summary>
public class ColumnResponse
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public int Order { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<TaskResponse> Tasks { get; set; } = new();
}

/// <summary>
/// DTO для изменения порядка колонок
/// </summary>
public class ReorderColumnsRequest
{
    [Required(ErrorMessage = "Список колонок обязателен")]
    public List<ColumnOrderItem> Columns { get; set; } = new();
}

/// <summary>
/// Элемент для изменения порядка колонки
/// </summary>
public class ColumnOrderItem
{
    [Required(ErrorMessage = "ID колонки обязателен")]
    public string Id { get; set; } = string.Empty;
    
    public int Order { get; set; } = 0;
}

/// <summary>
/// DTO для краткого ответа с данными колонки (без задач)
/// </summary>
public class ColumnSummaryResponse
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public int Order { get; set; } = 0;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int TaskCount { get; set; } = 0;
} 