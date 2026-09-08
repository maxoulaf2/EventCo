using EventCo.Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace EventCo.Infrastructure.Realtime;

internal sealed class TaskRealtimeNotifier(IHubContext<EventHub> hubContext) : ITaskRealtimeNotifier
{
    public Task NotifyTaskCreated(TaskRealtimeDto task, CancellationToken cancellationToken) =>
        Send(task.EventId, "TaskCreated", task, cancellationToken);

    public Task NotifyTaskAssigned(TaskRealtimeDto task, CancellationToken cancellationToken) =>
        Send(task.EventId, "TaskAssigned", task, cancellationToken);

    public Task NotifyTaskStatusChanged(TaskRealtimeDto task, CancellationToken cancellationToken) =>
        Send(task.EventId, "TaskStatusChanged", task, cancellationToken);

    public Task NotifyTaskDeleted(TaskDeletedRealtimeDto task, CancellationToken cancellationToken) =>
        Send(task.EventId, "TaskDeleted", task, cancellationToken);

    private Task Send(Guid eventId, string method, object payload, CancellationToken cancellationToken) =>
        hubContext.Clients.Group(EventHub.GroupName(eventId)).SendAsync(method, payload, cancellationToken);
}
