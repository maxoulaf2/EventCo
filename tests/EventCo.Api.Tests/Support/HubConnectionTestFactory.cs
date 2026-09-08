using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;

namespace EventCo.Api.Tests.Support;

// TestServer ne peut pas ouvrir de vrai socket TCP : on branche le client SignalR sur le
// WebSocketClient in-memory de TestServer, et on saute la négociation HTTP habituelle (pas de
// transport de repli à choisir hors WebSockets dans ce contexte de test).
internal static class HubConnectionTestFactory
{
    public static HubConnection Build(string? cookieHeader)
    {
        var server = Hooks.Factory.Server;
        var webSocketClient = server.CreateWebSocketClient();
        if (cookieHeader is not null)
        {
            webSocketClient.ConfigureRequest = request => request.Headers["Cookie"] = cookieHeader;
        }

        return new HubConnectionBuilder()
            .WithUrl(new Uri(server.BaseAddress, "/hubs/events"), options =>
            {
                options.SkipNegotiation = true;
                options.Transports = HttpTransportType.WebSockets;
                options.WebSocketFactory = async (context, cancellationToken) =>
                    await webSocketClient.ConnectAsync(context.Uri, cancellationToken);
            })
            .Build();
    }
}
