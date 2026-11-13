using EventCo.Domain.Common;

namespace EventCo.Domain.Events.DomainEvents;

public sealed record EventDateChosenDomainEvent(Guid EventId) : IDomainEvent;
