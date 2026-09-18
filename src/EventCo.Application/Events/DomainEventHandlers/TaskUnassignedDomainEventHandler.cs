using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events.DomainEvents;

namespace EventCo.Application.Events.DomainEventHandlers;

public sealed class TaskUnassignedDomainEventHandler(ITaskRealtimeNotifier taskRealtimeNotifier)
    : IDomainEventHandler<TaskUnassignedDomainEvent>
{
    public Task Handle(TaskUnassignedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        taskRealtimeNotifier.NotifyTaskUnassigned(TaskRealtimeDto.FromTask(domainEvent.Task), cancellationToken);
}
