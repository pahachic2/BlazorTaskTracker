using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TaskTracker.Models;

/// <summary>
/// Модель связи пользователя с организацией для MongoDB
/// </summary>
public class UserOrganization
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("organizationId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string OrganizationId { get; set; } = string.Empty;

    [BsonElement("role")]
    public OrganizationRole Role { get; set; } = OrganizationRole.Member;

    [BsonElement("joinedAt")]
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Роли пользователей в организации
/// </summary>
public enum OrganizationRole
{
    Owner = 1,
    Admin = 2,
    Member = 3
} 