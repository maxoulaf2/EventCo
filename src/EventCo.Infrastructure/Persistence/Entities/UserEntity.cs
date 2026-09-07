namespace EventCo.Infrastructure.Persistence.Entities;

public class UserEntity
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}
