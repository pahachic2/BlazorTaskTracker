namespace TaskTracker.Models
{
    public class Project
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Color { get; set; } = "bg-blue-500"; // Цвет для идентификации проекта
        public string Icon { get; set; } = "📋"; // Эмодзи иконка для проекта
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public List<string> Members { get; set; } = new();
        public int TaskCount { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }
} 