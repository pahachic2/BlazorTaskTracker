using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TaskTracker.Models
{
    /// <summary>
    /// Модель для хранения Kanban доски проекта в MongoDB
    /// </summary>
    public class KanbanBoardData
    {
        public KanbanBoardData()
        {
            Id = "";
            ProjectId = "";
            Columns = new List<KanbanColumn>();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        [BsonId]
        [BsonRepresentation(BsonType.String)]
        [BsonElement("id")]
        public string Id { get; set; }

        [BsonElement("projectId")]
        public string ProjectId { get; set; }

        [BsonElement("columns")]
        public List<KanbanColumn> Columns { get; set; }

        [BsonElement("createdAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }

        [BsonElement("updatedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime UpdatedAt { get; set; }
    }

    public class KanbanColumn
    {
        public KanbanColumn()
        {
            Id = "";
            Title = "";
            Tasks = new List<KanbanTask>();
            Order = 0;
        }

        [BsonElement("id")]
        public string Id { get; set; }

        [BsonElement("title")]
        public string Title { get; set; }

        [BsonElement("tasks")]
        public List<KanbanTask> Tasks { get; set; }

        [BsonElement("order")]
        public int Order { get; set; } // Порядок колонки на доске
    }
    
    public class KanbanTask
    {
        public KanbanTask()
        {
            Id = "";
            Title = "";
            Description = "";
            Tags = new List<string>();
            Assignees = new List<string>();
            ColumnId = "";
            Priority = TaskPriority.Normal;
            Status = TaskStatus.Active;
        }

        [BsonId]
        [BsonRepresentation(BsonType.String)]
        [BsonElement("id")]
        public string Id { get; set; }

        [BsonElement("title")]
        public string Title { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("tags")]
        public List<string> Tags { get; set; }

        [BsonElement("assignees")]
        public List<string> Assignees { get; set; }

        [BsonElement("dueDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? DueDate { get; set; }

        [BsonElement("columnId")]
        public string ColumnId { get; set; }

        [BsonElement("priority")]
        [BsonRepresentation(BsonType.String)]
        public TaskPriority Priority { get; set; }

        [BsonElement("status")]
        [BsonRepresentation(BsonType.String)]
        public TaskStatus Status { get; set; }

        [BsonElement("createdAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("order")]
        public int Order { get; set; } // Порядок задачи в колонке
    }

    /// <summary>
    /// Приоритет задачи
    /// </summary>
    public enum TaskPriority
    {
        Low,
        Normal,
        High,
        Critical
    }

    /// <summary>
    /// Статус задачи
    /// </summary>
    public enum TaskStatus
    {
        Active,
        Archived,
        Deleted
    }
    
    // Класс для передачи данных о перемещении задачи (не сохраняется в БД)
    public class TaskMovedEventArgs
    {
        public TaskMovedEventArgs()
        {
            TaskId = "";
            FromColumnId = "";
            ToColumnId = "";
        }

        public string TaskId { get; set; }
        public string FromColumnId { get; set; }
        public string ToColumnId { get; set; }
        public int? NewOrder { get; set; } // Новый порядок в колонке
    }
} 