using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Events.PromoteToOrganizer;

public sealed record PromoteToOrganizerCommand(Guid EventId, Guid TargetUserId) : ICommand;
