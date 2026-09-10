using EventCo.Domain.Common;

namespace EventCo.Domain.Events.DomainEvents;

public sealed record EventDetailsUpdatedDomainEvent(Guid EventId) : IDomainEvent;
