using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events.DomainEvents;

namespace EventCo.Application.Events.DomainEvents;

public sealed class TaskDeletedDomainEventHandler(ITaskRealtimeNotifier taskRealtimeNotifier)
    : IDomainEventHandler<TaskDeletedDomainEvent>
{
    public Task Handle(TaskDeletedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        taskRealtimeNotifier.NotifyTaskDeleted(
            new TaskDeletedRealtimeDto(domainEvent.EventId, domainEvent.TaskId),
            cancellationToken);
}
