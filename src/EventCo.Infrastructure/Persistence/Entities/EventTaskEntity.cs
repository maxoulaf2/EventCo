using EventCo.Domain.Events;

namespace EventCo.Infrastructure.Persistence.Entities;

public class EventTaskEntity
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string Title { get; set; } = null!;
    public TaskCategory Category { get; set; }
    public string? Quantity { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
}
