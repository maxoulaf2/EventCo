using EventCo.Domain.Common;

namespace EventCo.Domain.Events.DomainEvents;

public sealed record EventStatusChangedDomainEvent(Guid EventId) : IDomainEvent;
