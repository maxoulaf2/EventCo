using EventCo.Domain.Events;

namespace EventCo.Infrastructure.Persistence.Entities;

public class EventEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public string? Location { get; set; }
    public string? ImageUrl { get; set; }
    public Guid CreatedByUserId { get; set; }
    public EventStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<EventParticipantEntity> Participants { get; set; } = [];
    public List<EventTaskEntity> Tasks { get; set; } = [];
}
