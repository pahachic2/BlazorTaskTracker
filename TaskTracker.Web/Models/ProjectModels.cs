namespace TaskTracker.Web.Models
{
    public class Project
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty; // Added
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; } // Made nullable, removed default initializer for null
        public string Color { get; set; } = "bg-blue-500"; // Цвет для идентификации проекта
        public string Icon { get; set; } = "📋"; // Эмодзи иконка для проекта
        public DateTime CreatedAt { get; set; } = DateTime.Now; // Renamed
        public List<string> Members { get; set; } = new();
        public int TaskCount { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }
} 