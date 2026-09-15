using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events.DomainEvents;

namespace EventCo.Application.Events.DomainEventHandlers;

public sealed class TaskStatusChangedDomainEventHandler(ITaskRealtimeNotifier taskRealtimeNotifier)
    : IDomainEventHandler<TaskStatusChangedDomainEvent>
{
    public Task Handle(TaskStatusChangedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        taskRealtimeNotifier.NotifyTaskStatusChanged(TaskRealtimeDto.FromTask(domainEvent.Task), cancellationToken);
}
