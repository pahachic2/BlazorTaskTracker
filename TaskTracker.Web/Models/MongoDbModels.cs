using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TaskTracker.Web.Models
{
    /// <summary>
    /// DTO для создания нового пользователя
    /// </summary>
    public class CreateUserDto
    {
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
    }

    /// <summary>
    /// DTO для создания нового проекта
    /// </summary>
    public class CreateProjectDto
    {
        public string Name { get; set; } = "";
        public List<ObjectId> MemberIds { get; set; } = new();
    }

    /// <summary>
    /// DTO для создания новой колонки
    /// </summary>
    public class CreateColumnDto
    {
        public string Title { get; set; } = "";
        public int OrderIndex { get; set; }
    }

    /// <summary>
    /// DTO для создания новой задачи
    /// </summary>
    public class CreateTaskCardDto
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public ObjectId? AssigneeId { get; set; }
        public DateTime? DueDate { get; set; }
        public int OrderIndex { get; set; }
    }

    /// <summary>
    /// DTO для обновления задачи
    /// </summary>
    public class UpdateTaskCardDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public ObjectId? AssigneeId { get; set; }
        public DateTime? DueDate { get; set; }
        public int? OrderIndex { get; set; }
    }

    /// <summary>
    /// DTO для перемещения задачи между колонками
    /// </summary>
    public class MoveTaskDto
    {
        public ObjectId TaskId { get; set; }
        public ObjectId FromColumnId { get; set; }
        public ObjectId ToColumnId { get; set; }
        public int NewOrderIndex { get; set; }
    }

    /// <summary>
    /// Модель для настроек MongoDB коллекций
    /// </summary>
    public static class MongoCollections
    {
        public const string Users = "users";
        public const string Projects = "projects";
    }

    /// <summary>
    /// Вспомогательный класс для работы с ObjectId
    /// </summary>
    public static class ObjectIdExtensions
    {
        public static bool IsValidObjectId(string id)
        {
            return ObjectId.TryParse(id, out _);
        }

        public static ObjectId ToObjectId(this string id)
        {
            return ObjectId.TryParse(id, out var objectId) ? objectId : ObjectId.Empty;
        }

        public static string ToHexString(this ObjectId objectId)
        {
            return objectId.ToString();
        }
    }

    /// <summary>
    /// Модель ответа API с информацией о пользователе
    /// </summary>
    public class UserResponse
    {
        public string Id { get; set; } = "";
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
        public List<string> OwnedProjectIds { get; set; } = new();
        public List<string> ParticipatingProjectIds { get; set; } = new();

        public static UserResponse FromUser(User user)
        {
            return new UserResponse
            {
                Id = user.Id.ToString(),
                Username = user.Username,
                Email = user.Email,
                OwnedProjectIds = user.OwnedProjectIds.Select(id => id.ToString()).ToList(),
                ParticipatingProjectIds = user.ParticipatingProjectIds.Select(id => id.ToString()).ToList()
            };
        }
    }

    /// <summary>
    /// Модель ответа API с краткой информацией о проекте
    /// </summary>
    public class ProjectSummaryResponse
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string OwnerId { get; set; } = "";
        public List<string> MemberIds { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int ColumnsCount { get; set; }
        public int TasksCount { get; set; }

        public static ProjectSummaryResponse FromProject(Project project)
        {
            return new ProjectSummaryResponse
            {
                Id = project.Id.ToString(),
                Name = project.Name,
                OwnerId = project.OwnerId.ToString(),
                MemberIds = project.MemberIds.Select(id => id.ToString()).ToList(),
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt,
                ColumnsCount = project.Columns.Count,
                TasksCount = project.Columns.Sum(c => c.Tasks.Count)
            };
        }
    }
} 