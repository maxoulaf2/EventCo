using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events.Exceptions;

namespace EventCo.Application.Events.GetEventItems;

public sealed class GetEventItemsQueryHandler(
    ICurrentUserService currentUserService,
    IEventRepository eventRepository,
    IUserRepository userRepository) : ICommandHandler<GetEventItemsQuery, GetEventItemsResult>
{
    public async Task<GetEventItemsResult> Handle(GetEventItemsQuery request, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(request.EventId, cancellationToken)
            ?? throw new EventNotFoundException(request.EventId);

        var currentUserId = currentUserService.UserId!.Value;
        var currentUser = await userRepository.GetByIdAsync(currentUserId, cancellationToken);
        @event.EnsureCanBeViewedBy(currentUserId, currentUser?.IsAdmin == true);

        var items = @event.Items
            .OrderBy(t => t.CreatedAt)
            .Select(t => new EventItemSummary(
                t.Id,
                t.EventId,
                t.Title,
                t.Quantity,
                t.AssignedToUserId,
                t.CreatedAt))
            .ToList();

        return new GetEventItemsResult(items);
    }
}
