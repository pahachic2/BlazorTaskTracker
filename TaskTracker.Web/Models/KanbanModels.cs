namespace TaskTracker.Web.Models
{
    public class KanbanColumn
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty; // Renamed from Title
        public int Order { get; set; } // Added
        public string ProjectId { get; set; } = string.Empty; // Added
        public List<KanbanTask> Tasks { get; set; } = new();
    }
    
    public class KanbanTask
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; } // Made nullable
        public int Order { get; set; } // Added
        public DateTime CreatedAt { get; set; } = DateTime.Now; // Added
        public DateTime? DueDate { get; set; }
        public List<string>? AssigneeIds { get; set; } // Renamed from Assignees, made nullable
        public List<string>? Tags { get; set; } // Made nullable
        public string ColumnId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty; // Added
    }
    
    // Класс для передачи данных о перемещении задачи
    public class TaskMovedEventArgs
    {
        public string TaskId { get; set; } = "";
        public string FromColumnId { get; set; } = "";
        public string ToColumnId { get; set; } = "";
    }
} 