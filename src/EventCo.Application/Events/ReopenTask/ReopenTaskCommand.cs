using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Events.ReopenTask;

public sealed record ReopenTaskCommand(Guid EventId, Guid TaskId) : ICommand;
