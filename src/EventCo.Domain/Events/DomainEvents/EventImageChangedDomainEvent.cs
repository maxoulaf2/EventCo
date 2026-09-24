using EventCo.Domain.Common;

namespace EventCo.Domain.Events.DomainEvents;

public sealed record EventImageChangedDomainEvent(Guid EventId) : IDomainEvent;
