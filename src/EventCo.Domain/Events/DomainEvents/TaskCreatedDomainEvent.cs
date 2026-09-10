using EventCo.Domain.Common;

namespace EventCo.Domain.Events.DomainEvents;

public sealed record TaskCreatedDomainEvent(EventTask Task) : IDomainEvent;
