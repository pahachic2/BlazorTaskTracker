using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TaskTracker.Models;

public class Organization
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    
    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;
    
    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;
    
    [BsonElement("icon")]
    public string Icon { get; set; } = "🏢";
    
    [BsonElement("color")]
    public string Color { get; set; } = "bg-blue-500";
    
    [BsonElement("members")]
    public List<string> Members { get; set; } = new();
    
    [BsonElement("ownerId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string OwnerId { get; set; } = string.Empty;
    
    [BsonElement("projectCount")]
    public int ProjectCount { get; set; }
    
    [BsonElement("createdDate")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
} 