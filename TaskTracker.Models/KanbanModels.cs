using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TaskTracker.Models;

/// <summary>
/// Модель колонки канбан доски для MongoDB
/// </summary>
public class KanbanColumn
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("projectId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ProjectId { get; set; } = string.Empty;

    [BsonElement("order")]
    public int Order { get; set; } = 0;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Это поле не будет храниться в MongoDB - только для UI
    [BsonIgnore]
    public List<KanbanTask> Tasks { get; set; } = new();
}

/// <summary>
/// Модель задачи канбан доски для MongoDB
/// </summary>
public class KanbanTask
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("tags")]
    public List<string> Tags { get; set; } = new();

    [BsonElement("assignees")]
    public List<string> Assignees { get; set; } = new();

    [BsonElement("dueDate")]
    public DateTime? DueDate { get; set; }

    [BsonElement("columnId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ColumnId { get; set; } = string.Empty;

    [BsonElement("projectId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ProjectId { get; set; } = string.Empty;

    [BsonElement("createdBy")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string CreatedBy { get; set; } = string.Empty;

    [BsonElement("order")]
    public int Order { get; set; } = 0;

    [BsonElement("priority")]
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    [BsonElement("status")]
    public TaskStatus Status { get; set; } = TaskStatus.ToDo;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Перечисление приоритетов задач
/// </summary>
public enum TaskPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

/// <summary>
/// Перечисление статусов задач
/// </summary>
public enum TaskStatus
{
    ToDo = 1,
    InProgress = 2,
    Review = 3,
    Done = 4,
    Archived = 5
}

/// <summary>
/// Класс для передачи данных о перемещении задачи между колонками (не для MongoDB)
/// </summary>
public class TaskMovedEventArgs
{
    public string TaskId { get; set; } = string.Empty;
    public string FromColumnId { get; set; } = string.Empty;
    public string ToColumnId { get; set; } = string.Empty;
} 