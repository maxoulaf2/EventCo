using EventCo.Domain.Common;

namespace EventCo.Domain.Events.DomainEvents;

public sealed record ParticipantRoleChangedDomainEvent(Guid EventId, Guid ParticipantId) : IDomainEvent;
