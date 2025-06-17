using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TaskTracker.Models;

/// <summary>
/// Модель приглашения в организацию для MongoDB
/// </summary>
public class OrganizationInvitation
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("organizationId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string OrganizationId { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("invitedBy")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string InvitedBy { get; set; } = string.Empty;

    [BsonElement("role")]
    public OrganizationRole Role { get; set; } = OrganizationRole.Member;

    [BsonElement("token")]
    public string Token { get; set; } = string.Empty;

    [BsonElement("status")]
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("expiresAt")]
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7); // Приглашение действует 7 дней

    [BsonElement("acceptedAt")]
    public DateTime? AcceptedAt { get; set; }

    [BsonElement("acceptedBy")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? AcceptedBy { get; set; } // ID пользователя, который принял приглашение

    [BsonElement("emailSent")]
    public bool EmailSent { get; set; } = false; // Был ли отправлен email

    [BsonElement("userWasRegistered")]
    public bool UserWasRegistered { get; set; } = false; // Был ли пользователь зарегистрирован на момент приглашения
}

/// <summary>
/// Статусы приглашений в организацию
/// </summary>
public enum InvitationStatus
{
    Pending = 1,    // Ожидает принятия
    Accepted = 2,   // Принято
    Declined = 3,   // Отклонено
    Expired = 4,    // Истекло
    Revoked = 5     // Отозвано
} 