using System.Security.Claims;
using EventCo.Application.Common.Interfaces;
using EventCo.Domain.Common;
using EventCo.Domain.Events.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace EventCo.Infrastructure.Realtime;

// Une connexion = un utilisateur qui peut suivre plusieurs événements à la fois (plusieurs onglets/events
// ouverts) : le regroupement se fait par événement (groupe SignalR), pas par utilisateur ou par connexion.
[Authorize]
public sealed class EventHub(IEventRepository eventRepository, IUserRepository userRepository) : Hub
{
    public static string GroupName(Guid eventId) => $"event-{eventId}";

    public async Task JoinEvent(Guid eventId)
    {
        try
        {
            var @event = await eventRepository.GetByIdAsync(eventId, Context.ConnectionAborted)
                ?? throw new EventNotFoundException(eventId);

            var userId = GetUserId();
            var user = await userRepository.GetByIdAsync(userId, Context.ConnectionAborted);
            @event.EnsureCanBeViewedBy(userId, user?.IsAdmin == true);
        }
        catch (DomainException ex)
        {
            throw new HubException(ex.Message);
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(eventId), Context.ConnectionAborted);
    }

    public Task LeaveEvent(Guid eventId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(eventId), Context.ConnectionAborted);

    private Guid GetUserId()
    {
        var value = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(value, out var userId) ? userId : throw new HubException("Utilisateur non authentifié.");
    }
}
