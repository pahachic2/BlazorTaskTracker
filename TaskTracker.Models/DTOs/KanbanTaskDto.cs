using System;
using System.Collections.Generic;

namespace TaskTracker.Models.DTOs
{
    public class KanbanTaskDto
    {
        public string Id { get; set; } = string.Empty;
        public string ColumnId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Order { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public List<string>? AssigneeIds { get; set; }
        public List<string>? Tags { get; set; }
    }
}
