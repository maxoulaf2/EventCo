using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Events.CreateTask;

public sealed record CreateTaskCommand(Guid EventId, string Title, string Category, string? Quantity) : ICommand<CreateTaskResult>;
