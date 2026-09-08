using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Events.DeleteTask;

public sealed record DeleteTaskCommand(Guid EventId, Guid TaskId) : ICommand;
