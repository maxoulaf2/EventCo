using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Events.AssignTask;

public sealed record AssignTaskCommand(Guid EventId, Guid TaskId, Guid UserId) : ICommand;
