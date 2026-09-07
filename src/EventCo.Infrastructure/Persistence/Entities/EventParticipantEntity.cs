using EventCo.Domain.Events;

namespace EventCo.Infrastructure.Persistence.Entities;

public class EventParticipantEntity
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public ParticipantRole Role { get; set; }
    public DateTime InvitedAt { get; set; }
    public DateTime? JoinedAt { get; set; }
}
