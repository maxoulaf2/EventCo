using EventCo.Domain.Common;

namespace EventCo.Domain.Auth.DomainEvents;

public sealed record MagicLinkTokenFailedAttemptRegisteredDomainEvent(Guid TokenId) : IDomainEvent;
