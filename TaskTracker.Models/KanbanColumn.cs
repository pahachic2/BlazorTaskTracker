using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace TaskTracker.Models
{
    public class KanbanColumn
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)] // Assuming ProjectId is an ObjectId string
        [BsonElement("projectId")]
        public string ProjectId { get; set; } = string.Empty;

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("order")]
        public int Order { get; set; }
    }
}
