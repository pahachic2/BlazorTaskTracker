using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace TaskTracker.Models
{
    public class KanbanTask
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)] // Assuming ColumnId is an ObjectId string
        [BsonElement("columnId")]
        public string ColumnId { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)] // Assuming ProjectId is an ObjectId string
        [BsonElement("projectId")]
        public string ProjectId { get; set; } = string.Empty;

        [BsonElement("title")]
        public string Title { get; set; } = string.Empty;

        [BsonElement("description")]
        public string? Description { get; set; }

        [BsonElement("order")]
        public int Order { get; set; } // Order within the column

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("dueDate")]
        [BsonIgnoreIfNull]
        public DateTime? DueDate { get; set; }

        [BsonElement("assigneeIds")]
        [BsonIgnoreIfNull] // Assuming assignees are user IDs (ObjectIds)
        public List<string>? AssigneeIds { get; set; }

        [BsonElement("tags")]
        [BsonIgnoreIfNull]
        public List<string>? Tags { get; set; }
    }
}
