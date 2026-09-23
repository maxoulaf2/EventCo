using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Users.Exceptions;

namespace EventCo.Application.Events.GetAllEvents;

// Le flag administrateur est relu en base à chaque appel (pas porté par le cookie de session) :
// un retrait du flag prend effet immédiatement, sans attendre l'expiration de la session.
public sealed class GetAllEventsQueryHandler(
    ICurrentUserService currentUserService,
    IUserRepository userRepository,
    IEventRepository eventRepository) : ICommandHandler<GetAllEventsQuery, GetAllEventsResult>
{
    public async Task<GetAllEventsResult> Handle(GetAllEventsQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = currentUserService.UserId!.Value;
        var currentUser = await userRepository.GetByIdAsync(currentUserId, cancellationToken);
        if (currentUser is not { IsAdmin: true })
            throw new UserNotAdminException(currentUserId);

        var events = await eventRepository.GetAllAsync(cancellationToken);

        var overviews = events
            .Select(@event => new EventOverview(
                @event.Id,
                @event.Title,
                @event.EventDate,
                @event.Location,
                @event.CreatedByUserId,
                @event.Status.ToString(),
                @event.Participants.Count))
            .ToList();

        return new GetAllEventsResult(overviews);
    }
}
