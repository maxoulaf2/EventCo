using EventCo.Application.Common.Messaging;

namespace EventCo.Application.Events.GetEventTasks;

public sealed record GetEventTasksQuery(Guid EventId) : ICommand<GetEventTasksResult>;
