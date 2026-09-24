using Microsoft.AspNetCore.SignalR.Client;

namespace EventCo.Api.Tests.Support;

// Injectée par Reqnroll (context injection), pour partager la connexion SignalR ouverte via
// JoinEventHubSteps avec les autres classes [Binding] du même scénario (ex: ItemRealtimeNotificationsSteps).
// Reqnroll dispose ce contexte (IAsyncDisposable) à la fin de chaque scénario, comme il le ferait
// pour une classe [Binding] : la connexion n'a donc pas besoin d'être fermée explicitement ailleurs.
// Nommée "Realtime..." (et non "HubConnectionContext" tout court) pour éviter une collision avec
// le type serveur Microsoft.AspNetCore.SignalR.HubConnectionContext, déjà en scope via EventHub.
public sealed class RealtimeHubConnectionContext : IAsyncDisposable
{
    public HubConnection? Connection { get; set; }

    public async ValueTask DisposeAsync()
    {
        if (Connection is not null)
        {
            await Connection.DisposeAsync();
        }
    }
}
