using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events.DomainEvents;

namespace EventCo.Application.Events.DomainEventHandlers;

public sealed class TaskAssignedDomainEventHandler(ITaskRealtimeNotifier taskRealtimeNotifier)
    : IDomainEventHandler<TaskAssignedDomainEvent>
{
    public Task Handle(TaskAssignedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        taskRealtimeNotifier.NotifyTaskAssigned(TaskRealtimeDto.FromTask(domainEvent.Task), cancellationToken);
}


