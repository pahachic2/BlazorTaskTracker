using System.Collections.Generic;

namespace TaskTracker.Models.DTOs
{
    public class KanbanBoardDto
    {
        public ProjectDto Project { get; set; } = null!; // Or initialize with new ProjectDto() if preferred
        public List<KanbanColumnDto> Columns { get; set; } = new List<KanbanColumnDto>();
    }
}
