using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options; // Added for IOptions
using MongoDB.Driver;
using TaskTracker.Api.Configuration; // Added for DatabaseSettings
using TaskTracker.Models;
using TaskTracker.Models.DTOs;

namespace TaskTracker.Api.Services
{
    public class KanbanService : IKanbanService
    {
        private readonly IMongoCollection<Project> _projects;
        private readonly IMongoCollection<KanbanColumn> _columns;
        private readonly IMongoCollection<KanbanTask> _tasks;

        // Modified constructor
        public KanbanService(IOptions<DatabaseSettings> databaseSettings)
        {
            var mongoClient = new MongoClient(databaseSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(databaseSettings.Value.DatabaseName);
            _projects = mongoDatabase.GetCollection<Project>("projects");
            _columns = mongoDatabase.GetCollection<KanbanColumn>("columns");
            _tasks = mongoDatabase.GetCollection<KanbanTask>("tasks");
        }

        public async Task<IEnumerable<ProjectDto>> GetProjectsByUserIdAsync(string userId)
        {
            var projects = await _projects.Find(p => p.UserId == userId).ToListAsync();
            // Manual mapping (AutoMapper could be used in a real project)
            return projects.Select(p => new ProjectDto
            {
                Id = p.Id,
                UserId = p.UserId,
                Name = p.Name,
                Description = p.Description,
                CreatedAt = p.CreatedAt
            }).ToList();
        }

        public async Task<KanbanBoardDto?> GetBoardByProjectIdAsync(string projectId, string userId)
        {
            var project = await _projects.Find(p => p.Id == projectId && p.UserId == userId).FirstOrDefaultAsync();
            if (project == null)
            {
                return null; // Or throw NotFoundException
            }

            var projectDto = new ProjectDto
            {
                Id = project.Id,
                UserId = project.UserId,
                Name = project.Name,
                Description = project.Description,
                CreatedAt = project.CreatedAt
            };

            var columns = await _columns.Find(c => c.ProjectId == projectId).SortBy(c => c.Order).ToListAsync();
            var columnDtos = new List<KanbanColumnDto>();

            foreach (var column in columns)
            {
                var tasks = await _tasks.Find(t => t.ColumnId == column.Id).SortBy(t => t.Order).ToListAsync();
                var taskDtos = tasks.Select(t => new KanbanTaskDto
                {
                    Id = t.Id,
                    ColumnId = t.ColumnId,
                    ProjectId = t.ProjectId,
                    Title = t.Title,
                    Description = t.Description,
                    Order = t.Order,
                    CreatedAt = t.CreatedAt,
                    DueDate = t.DueDate,
                    AssigneeIds = t.AssigneeIds,
                    Tags = t.Tags
                }).ToList();

                columnDtos.Add(new KanbanColumnDto
                {
                    Id = column.Id,
                    ProjectId = column.ProjectId,
                    Name = column.Name,
                    Order = column.Order,
                    Tasks = taskDtos
                });
            }

            return new KanbanBoardDto
            {
                Project = projectDto,
                Columns = columnDtos
            };
        }
    }
}
