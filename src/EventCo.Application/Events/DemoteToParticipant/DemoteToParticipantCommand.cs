using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Events.DemoteToParticipant;

public sealed record DemoteToParticipantCommand(Guid EventId, Guid TargetUserId) : ICommand;
