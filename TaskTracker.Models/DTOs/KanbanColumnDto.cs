using System;
using System.Collections.Generic;

namespace TaskTracker.Models.DTOs
{
    public class KanbanColumnDto
    {
        public string Id { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Order { get; set; }
        public List<KanbanTaskDto> Tasks { get; set; } = new List<KanbanTaskDto>();
    }
}
