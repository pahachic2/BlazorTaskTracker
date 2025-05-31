using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TaskTracker.Web.Models
{
    /// <summary>
    /// Модель пользователя для MongoDB
    /// </summary>
    public class User
    {
        [BsonId]
        [BsonElement("_id")]
        public ObjectId Id { get; set; }

        [BsonElement("username")]
        public string Username { get; set; } = "";

        [BsonElement("email")]
        public string Email { get; set; } = "";

        [BsonElement("ownedProjectIds")]
        public List<ObjectId> OwnedProjectIds { get; set; } = new();

        [BsonElement("participatingProjectIds")]
        public List<ObjectId> ParticipatingProjectIds { get; set; } = new();

        public User()
        {
            Id = ObjectId.GenerateNewId();
        }
    }

    /// <summary>
    /// Модель проекта для MongoDB с вложенными колонками
    /// </summary>
    public class Project
    {
        [BsonId]
        [BsonElement("_id")]
        public ObjectId Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; } = "";

        [BsonElement("ownerId")]
        public ObjectId OwnerId { get; set; }

        [BsonElement("memberIds")]
        public List<ObjectId> MemberIds { get; set; } = new();

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("updatedAt")]
        public DateTime? UpdatedAt { get; set; }

        [BsonElement("columns")]
        public List<Column> Columns { get; set; } = new();

        public Project()
        {
            Id = ObjectId.GenerateNewId();
            CreatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Модель колонки как вложенного документа
    /// </summary>
    public class Column
    {
        [BsonElement("id")]
        public ObjectId Id { get; set; }

        [BsonElement("title")]
        public string Title { get; set; } = "";

        [BsonElement("orderIndex")]
        public int OrderIndex { get; set; }

        [BsonElement("tasks")]
        public List<TaskCard> Tasks { get; set; } = new();

        public Column()
        {
            Id = ObjectId.GenerateNewId();
        }
    }

    /// <summary>
    /// Модель задачи как вложенного документа
    /// </summary>
    public class TaskCard
    {
        [BsonElement("id")]
        public ObjectId Id { get; set; }

        [BsonElement("title")]
        public string Title { get; set; } = "";

        [BsonElement("description")]
        public string Description { get; set; } = "";

        [BsonElement("assigneeId")]
        public ObjectId? AssigneeId { get; set; }

        [BsonElement("dueDate")]
        public DateTime? DueDate { get; set; }

        [BsonElement("orderIndex")]
        public int OrderIndex { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("updatedAt")]
        public DateTime? UpdatedAt { get; set; }

        public TaskCard()
        {
            Id = ObjectId.GenerateNewId();
            CreatedAt = DateTime.UtcNow;
        }
    }
} 