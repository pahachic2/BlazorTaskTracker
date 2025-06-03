namespace TaskTracker.Models.DTOs;

public class OrganizationResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public List<string> Members { get; set; } = new();
    public string OwnerId { get; set; } = string.Empty;
    public int ProjectCount { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CreateOrganizationRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "🏢";
    public string Color { get; set; } = "bg-blue-500";
}

public class UpdateOrganizationRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
} 