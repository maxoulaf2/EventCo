using EventCo.Domain.Common;

namespace EventCo.Domain.Events.DomainEvents;

public sealed record ParticipantParticipationStatusChangedDomainEvent(Guid EventId, Guid ParticipantId) : IDomainEvent;
