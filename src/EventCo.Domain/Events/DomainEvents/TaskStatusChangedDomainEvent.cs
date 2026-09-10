using EventCo.Domain.Common;

namespace EventCo.Domain.Events.DomainEvents;

public sealed record TaskStatusChangedDomainEvent(EventTask Task) : IDomainEvent;
