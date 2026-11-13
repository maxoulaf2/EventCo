using EventCo.Application.Common.Interfaces;

namespace EventCo.Application.Tests.TestDoubles;

public sealed class RecordingDatePollRealtimeNotifier : IDatePollRealtimeNotifier
{
    public List<Guid> ChangedEventIds { get; } = [];

    public Task NotifyDatePollChanged(Guid eventId, CancellationToken cancellationToken)
    {
        ChangedEventIds.Add(eventId);
        return Task.CompletedTask;
    }
}
