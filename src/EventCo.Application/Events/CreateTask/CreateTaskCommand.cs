using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Events.CreateTask;

public sealed record CreateTaskCommand(Guid EventId, string Title, string? Quantity) : ICommand<CreateTaskResult>;
