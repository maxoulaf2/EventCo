using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events;

namespace EventCo.Application.Events.CreateEvent;

// Le userId courant est garanti non nul par [Authorize] sur l'endpoint appelant.
public sealed class CreateEventCommandHandler(
    ICurrentUserService currentUserService,
    IEventRepository eventRepository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<CreateEventCommand, CreateEventResult>
{
    public async Task<CreateEventResult> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.UtcNow;
        var @event = Event.Create(
            request.Title,
            request.Description,
            request.EventDate,
            request.Location,
            currentUserService.UserId!.Value,
            now);

        await eventRepository.AddAsync(@event, cancellationToken);

        return new CreateEventResult(
            @event.Id,
            @event.Title,
            @event.Description,
            @event.EventDate,
            @event.Location,
            @event.CreatedByUserId,
            @event.Status.ToString(),
            @event.CreatedAt);
    }
}
