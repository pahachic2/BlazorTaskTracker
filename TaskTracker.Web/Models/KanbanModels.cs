namespace TaskTracker.Web.Models
{
    public class KanbanColumn
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public List<KanbanTask> Tasks { get; set; } = new();
    }
    
    public class KanbanTask
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public List<string> Tags { get; set; } = new();
        public List<string> Assignees { get; set; } = new();
        public DateTime? DueDate { get; set; }
        public string ColumnId { get; set; } = "";
    }
    
    // Класс для передачи данных о перемещении задачи
    public class TaskMovedEventArgs
    {
        public string TaskId { get; set; } = "";
        public string FromColumnId { get; set; } = "";
        public string ToColumnId { get; set; } = "";
    }
} 