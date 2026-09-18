namespace EventCo.Application.Common.Interfaces;

public interface ITaskRealtimeNotifier
{
    Task NotifyTaskCreated(TaskRealtimeDto task, CancellationToken cancellationToken);

    Task NotifyTaskAssigned(TaskRealtimeDto task, CancellationToken cancellationToken);

    Task NotifyTaskUnassigned(TaskRealtimeDto task, CancellationToken cancellationToken);

    Task NotifyTaskStatusChanged(TaskRealtimeDto task, CancellationToken cancellationToken);

    Task NotifyTaskDeleted(TaskDeletedRealtimeDto task, CancellationToken cancellationToken);
}
