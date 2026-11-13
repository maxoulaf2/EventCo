using EventCo.Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace EventCo.Infrastructure.Realtime;

internal sealed class DatePollRealtimeNotifier(IHubContext<EventHub> hubContext) : IDatePollRealtimeNotifier
{
    public Task NotifyDatePollChanged(Guid eventId, CancellationToken cancellationToken) =>
        hubContext.Clients.Group(EventHub.GroupName(eventId)).SendAsync("DatePollChanged", new { eventId }, cancellationToken);
}
