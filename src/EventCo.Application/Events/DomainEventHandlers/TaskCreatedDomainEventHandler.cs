using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events.DomainEvents;

namespace EventCo.Application.Events.DomainEventHandlers;

public sealed class TaskCreatedDomainEventHandler(ITaskRealtimeNotifier taskRealtimeNotifier)
    : IDomainEventHandler<TaskCreatedDomainEvent>
{
    public Task Handle(TaskCreatedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        taskRealtimeNotifier.NotifyTaskCreated(TaskRealtimeDto.FromTask(domainEvent.Task), cancellationToken);
}
