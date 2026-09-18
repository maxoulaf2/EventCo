using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Events.SetParticipationStatus;

public sealed record SetParticipationStatusCommand(Guid EventId, string Status) : ICommand;
