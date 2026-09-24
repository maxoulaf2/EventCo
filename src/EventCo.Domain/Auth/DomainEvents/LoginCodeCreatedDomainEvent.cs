using EventCo.Domain.Common;

namespace EventCo.Domain.Auth.DomainEvents;

public sealed record LoginCodeCreatedDomainEvent(Guid LoginCodeId) : IDomainEvent;
