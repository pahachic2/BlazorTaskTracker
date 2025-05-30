using System;

namespace TaskTracker.Models.DTOs
{
    public class ProjectDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty; // Keep UserId for potential frontend use if needed
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
