using EventCo.Domain.Common;

namespace EventCo.Domain.Events.DomainEvents;

public sealed record EventCreatedDomainEvent(Guid EventId) : IDomainEvent;
