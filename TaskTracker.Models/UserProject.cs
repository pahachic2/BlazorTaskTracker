using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TaskTracker.Models;

/// <summary>
/// Модель связи пользователя с проектом для MongoDB
/// </summary>
public class UserProject
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("projectId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ProjectId { get; set; } = string.Empty;

    [BsonElement("role")]
    public ProjectRole Role { get; set; } = ProjectRole.Member;

    [BsonElement("joinedAt")]
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Роли пользователей в проекте
/// </summary>
public enum ProjectRole
{
    Owner = 1,
    Admin = 2,
    Member = 3,
    Viewer = 4
} 