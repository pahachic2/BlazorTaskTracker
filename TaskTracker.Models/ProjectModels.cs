using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TaskTracker.Models
{
    public class Project
    {
        public Project()
        {
            Id = "";
            Name = "";
            Description = "";
            Color = "bg-blue-500";
            Icon = "📋";
            CreatedDate = DateTime.UtcNow;
            Members = new List<string>();
            TaskCount = 0;
            IsActive = true;
        }

        [BsonId]
        [BsonRepresentation(BsonType.String)]
        [BsonElement("id")]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("color")]
        public string Color { get; set; } // Цвет для идентификации проекта

        [BsonElement("icon")]
        public string Icon { get; set; } // Эмодзи иконка для проекта

        [BsonElement("createdDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedDate { get; set; }

        [BsonElement("members")]
        public List<string> Members { get; set; }

        [BsonElement("taskCount")]
        public int TaskCount { get; set; }

        [BsonElement("isActive")]
        public bool IsActive { get; set; }

        [BsonElement("updatedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
} 