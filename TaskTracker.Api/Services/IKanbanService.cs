using System.Collections.Generic;
using System.Threading.Tasks;
using TaskTracker.Models.DTOs;

namespace TaskTracker.Api.Services
{
    public interface IKanbanService
    {
        Task<IEnumerable<ProjectDto>> GetProjectsByUserIdAsync(string userId);
        Task<KanbanBoardDto?> GetBoardByProjectIdAsync(string projectId, string userId);
    }
}
