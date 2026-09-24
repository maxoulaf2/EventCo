using EventCo.Domain.Common;

namespace EventCo.Domain.Auth.DomainEvents;

public sealed record LoginCodeFailedAttemptRegisteredDomainEvent(Guid LoginCodeId) : IDomainEvent;
