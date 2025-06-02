using System.ComponentModel.DataAnnotations;

namespace TaskTracker.Models.DTOs;

/// <summary>
/// DTO для создания нового проекта
/// </summary>
public class CreateProjectDto
{
    [Required(ErrorMessage = "Название проекта обязательно")]
    [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
    public string Name { get; set; } = "";
    
    [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
    public string? Description { get; set; }
    
    public string Icon { get; set; } = "📋";
    public string Color { get; set; } = "bg-blue-500";
    public List<string> Members { get; set; } = new();
}

/// <summary>
/// DTO для обновления проекта
/// </summary>
public class UpdateProjectDto
{
    [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
    public string? Name { get; set; }
    
    [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
    public string? Description { get; set; }
    
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public List<string>? Members { get; set; }
    public bool? IsActive { get; set; }
}

/// <summary>
/// DTO для создания новой задачи
/// </summary>
public class CreateTaskDto
{
    [Required(ErrorMessage = "Название задачи обязательно")]
    [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов")]
    public string Title { get; set; } = "";
    
    [StringLength(1000, ErrorMessage = "Описание не должно превышать 1000 символов")]
    public string? Description { get; set; }
    
    [Required(ErrorMessage = "ID колонки обязателен")]
    public string ColumnId { get; set; } = "";
    
    public List<string> Tags { get; set; } = new();
    public List<string> Assignees { get; set; } = new();
    public DateTime? DueDate { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Normal;
}

/// <summary>
/// DTO для обновления задачи
/// </summary>
public class UpdateTaskDto
{
    [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов")]
    public string? Title { get; set; }
    
    [StringLength(1000, ErrorMessage = "Описание не должно превышать 1000 символов")]
    public string? Description { get; set; }
    
    public string? ColumnId { get; set; }
    public List<string>? Tags { get; set; }
    public List<string>? Assignees { get; set; }
    public DateTime? DueDate { get; set; }
    public TaskPriority? Priority { get; set; }
    public TaskStatus? Status { get; set; }
}

/// <summary>
/// DTO для перемещения задачи
/// </summary>
public class MoveTaskDto
{
    [Required(ErrorMessage = "ID целевой колонки обязателен")]
    public string ToColumnId { get; set; } = "";
    
    public int? NewOrder { get; set; }
}

/// <summary>
/// DTO для создания новой колонки
/// </summary>
public class CreateColumnDto
{
    [Required(ErrorMessage = "Название колонки обязательно")]
    [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
    public string Title { get; set; } = "";
    
    public int Order { get; set; }
}

/// <summary>
/// DTO для обновления колонки
/// </summary>
public class UpdateColumnDto
{
    [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
    public string? Title { get; set; }
    
    public int? Order { get; set; }
}

/// <summary>
/// DTO для ответа с информацией о проекте
/// </summary>
public class ProjectResponseDto
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Color { get; set; } = "";
    public string Icon { get; set; } = "";
    public DateTime CreatedDate { get; set; }
    public List<string> Members { get; set; } = new();
    public int TaskCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO для ответа с информацией о задаче
/// </summary>
public class TaskResponseDto
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public List<string> Tags { get; set; } = new();
    public List<string> Assignees { get; set; } = new();
    public DateTime? DueDate { get; set; }
    public string ColumnId { get; set; } = "";
    public TaskPriority Priority { get; set; }
    public TaskStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int Order { get; set; }
} 