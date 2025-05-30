using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace TaskTracker.Models
{
    public class Project
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)] // Assuming UserId is also an ObjectId string from the User collection
        [BsonElement("userId")]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("description")]
        public string? Description { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
