using EventCo.Domain.Common;

namespace EventCo.Domain.Events.DomainEvents;

public sealed record ParticipantJoinedDomainEvent(Guid EventId, Guid ParticipantId) : IDomainEvent;
