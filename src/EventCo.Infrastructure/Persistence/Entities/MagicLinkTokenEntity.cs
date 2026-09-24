namespace EventCo.Infrastructure.Persistence.Entities;

public class MagicLinkTokenEntity
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string TokenHash { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public DateTime? ConsumedAt { get; set; }
    public int FailedAttempts { get; set; }
    public string? EventInviteLinkToken { get; set; }
    public DateTime CreatedAt { get; set; }
}
