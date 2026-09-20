using EventCo.Application.Common.Interfaces;

namespace EventCo.Application.Tests.TestDoubles;

public sealed class RecordingTaskRealtimeNotifier : ITaskRealtimeNotifier
{
    public List<TaskRealtimeDto> TaskCreatedNotifications { get; } = [];
    public List<TaskRealtimeDto> TaskAssignedNotifications { get; } = [];
    public List<TaskRealtimeDto> TaskUnassignedNotifications { get; } = [];
    public List<TaskDeletedRealtimeDto> TaskDeletedNotifications { get; } = [];

    public Task NotifyTaskCreated(TaskRealtimeDto task, CancellationToken cancellationToken)
    {
        TaskCreatedNotifications.Add(task);
        return Task.CompletedTask;
    }

    public Task NotifyTaskAssigned(TaskRealtimeDto task, CancellationToken cancellationToken)
    {
        TaskAssignedNotifications.Add(task);
        return Task.CompletedTask;
    }

    public Task NotifyTaskUnassigned(TaskRealtimeDto task, CancellationToken cancellationToken)
    {
        TaskUnassignedNotifications.Add(task);
        return Task.CompletedTask;
    }

    public Task NotifyTaskDeleted(TaskDeletedRealtimeDto task, CancellationToken cancellationToken)
    {
        TaskDeletedNotifications.Add(task);
        return Task.CompletedTask;
    }
}
