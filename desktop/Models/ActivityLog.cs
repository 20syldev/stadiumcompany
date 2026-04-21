namespace StadiumCompany.Models;

public class ActivityLog
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public string? Details { get; set; }
    public string Source { get; set; } = "desktop";
    public DateTime CreatedAt { get; set; }

    // Joined from users table for display
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
}
