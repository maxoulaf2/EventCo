using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events;
using EventCo.Domain.Events.Exceptions;

namespace EventCo.Application.Events.CreateTask;

public sealed class CreateTaskCommandHandler(
    ICurrentUserService currentUserService,
    IEventRepository eventRepository,
    IDateTimeProvider dateTimeProvider,
    ITaskRealtimeNotifier taskRealtimeNotifier) : ICommandHandler<CreateTaskCommand, CreateTaskResult>
{
    public async Task<CreateTaskResult> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(request.EventId, cancellationToken)
            ?? throw new EventNotFoundException(request.EventId);

        var now = dateTimeProvider.UtcNow;
        var category = Enum.Parse<TaskCategory>(request.Category);

        var task = @event.AddTask(currentUserService.UserId!.Value, request.Title, category, request.Quantity, now);

        await eventRepository.UpdateAsync(@event, cancellationToken);
        await taskRealtimeNotifier.NotifyTaskCreated(TaskRealtimeDto.FromTask(task), cancellationToken);

        return new CreateTaskResult(
            task.Id,
            @event.Id,
            task.Title,
            task.Category.ToString(),
            task.Quantity,
            task.AssignedToUserId,
            task.IsDone,
            task.CreatedAt);
    }
}
