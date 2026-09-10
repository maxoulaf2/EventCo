using EventCo.Domain.Common;

namespace EventCo.Domain.Events.DomainEvents;

public sealed record ParticipantInvitedDomainEvent(Guid EventId, Guid ParticipantId) : IDomainEvent;
