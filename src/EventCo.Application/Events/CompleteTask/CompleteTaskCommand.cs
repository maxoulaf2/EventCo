using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Events.CompleteTask;

public sealed record CompleteTaskCommand(Guid EventId, Guid TaskId) : ICommand;
