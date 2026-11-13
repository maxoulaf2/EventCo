namespace EventCo.Application.Common.Interfaces;

public interface IDatePollRealtimeNotifier
{
    // Simple signal, sans contenu : les clients rechargent le détail de l'événement (options, votes,
    // date retenue et présences changent ensemble à la clôture).
    Task NotifyDatePollChanged(Guid eventId, CancellationToken cancellationToken);
}
