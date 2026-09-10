using EventCo.Domain.Common;

namespace EventCo.Domain.Events.DomainEvents;

public sealed record ParticipantRemovedDomainEvent(Guid EventId, Guid ParticipantId) : IDomainEvent;
