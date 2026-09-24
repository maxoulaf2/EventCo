using EventCo.Domain.Common;

namespace EventCo.Domain.Auth.DomainEvents;

public sealed record LoginCodeConsumedDomainEvent(Guid LoginCodeId) : IDomainEvent;
