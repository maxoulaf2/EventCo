using System.Globalization;
using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Domain.Events.Exceptions;

namespace EventCo.Application.Events.GetEventItems;

public sealed class GetEventItemsQueryHandler(
    ICurrentUserService currentUserService,
    IEventRepository eventRepository,
    IUserRepository userRepository) : ICommandHandler<GetEventItemsQuery, GetEventItemsResult>
{
    private static readonly StringComparer FrenchTitleComparer = StringComparer.Create(CultureInfo.GetCultureInfo("fr-FR"), ignoreCase: true);

    public async Task<GetEventItemsResult> Handle(GetEventItemsQuery request, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(request.EventId, cancellationToken)
            ?? throw new EventNotFoundException(request.EventId);

        var currentUserId = currentUserService.UserId!.Value;
        var currentUser = await userRepository.GetByIdAsync(currentUserId, cancellationToken);
        @event.EnsureCanBeViewedBy(currentUserId, currentUser?.IsAdmin == true);

        // Ordre d'affichage décidé ici, le frontend regroupe sans retrier : tout ce qui reste à prendre en tête
        // (mis en valeur à part), puis les catégories dans l'ordre de l'enum (celui d'un repas), puis l'ordre
        // alphabétique.
        var items = @event.Items
            .OrderBy(t => t.AssignedToUserId is not null)
            .ThenBy(t => t.Category)
            .ThenBy(t => t.Title, FrenchTitleComparer)
            .Select(t => new EventItemSummary(
                t.Id,
                t.EventId,
                t.Title,
                t.Quantity,
                t.Kind.ToString(),
                t.Category.ToString(),
                t.AssignedToUserId,
                t.CreatedAt))
            .ToList();

        return new GetEventItemsResult(items);
    }
}
