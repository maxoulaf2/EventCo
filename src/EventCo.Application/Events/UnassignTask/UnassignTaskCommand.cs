using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Events.UnassignTask;

public sealed record UnassignTaskCommand(Guid EventId, Guid TaskId) : ICommand;
