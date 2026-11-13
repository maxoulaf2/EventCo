using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Common.Security;
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
        var inviteLinkToken = SecureTokenGenerator.GenerateUrlSafeToken();
        var userId = currentUserService.UserId!.Value;

        // Date fixe ou sondage de date : l'un ou l'autre, garanti par CreateEventCommandValidator.
        var @event = request.EventDate is { } eventDate
            ? Event.Create(request.Title, request.Description, eventDate, request.Location, inviteLinkToken, userId, now)
            : Event.CreateWithDatePoll(request.Title, request.Description, request.DateOptions!, request.Location, inviteLinkToken, userId, now);

        await eventRepository.ApplyAsync(@event, cancellationToken);

        return new CreateEventResult(
            @event.Id,
            @event.Title,
            @event.Description,
            @event.EventDate,
            @event.Location,
            ImageUrl: null, // ajoutée après création, cf. UpdateEventImage
            @event.CreatedByUserId,
            @event.Status.ToString(),
            @event.CreatedAt);
    }
}
