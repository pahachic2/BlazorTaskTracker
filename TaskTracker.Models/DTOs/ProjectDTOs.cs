using System.ComponentModel.DataAnnotations;

namespace TaskTracker.Models.DTOs;

/// <summary>
/// DTO для создания проекта
/// </summary>
public class CreateProjectRequest
{
    [Required(ErrorMessage = "Название проекта обязательно")]
    [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
    public string? Description { get; set; }
    
    public string Icon { get; set; } = "📋";
    public string Color { get; set; } = "bg-blue-500";
    
    [Required(ErrorMessage = "ID организации обязателен")]
    public string OrganizationId { get; set; } = string.Empty;
}

/// <summary>
/// DTO для обновления проекта
/// </summary>
public class UpdateProjectRequest
{
    [Required(ErrorMessage = "Название проекта обязательно")]
    [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
    public string? Description { get; set; }
    
    public string Icon { get; set; } = "📋";
    public string Color { get; set; } = "bg-blue-500";
    public bool IsActive { get; set; } = true;
    
    public string OrganizationId { get; set; } = string.Empty;
}

/// <summary>
/// DTO для ответа с данными проекта
/// </summary>
public class ProjectResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string OrganizationId { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public List<string> Members { get; set; } = new();
    public int TaskCount { get; set; }
    public bool IsActive { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
} 