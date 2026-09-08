using EventCo.Api.Tests.Support;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Reqnroll;

namespace EventCo.Api.Tests.Events.JoinEventHub;

[Binding]
public sealed class JoinEventHubSteps(SessionContext sessionContext, EventContext eventContext) : IAsyncDisposable
{
    private HubConnection? _connection;
    private Exception? _joinError;
    private Exception? _connectError;

    [When(@"je me connecte au hub temps réel et que je rejoins cet événement")]
    public async Task QuandJeMeConnecteAuHubTempsReelEtQueJeRejoinsCetEvenement()
    {
        _connection = BuildConnection(sessionContext.Cookie);
        await _connection.StartAsync();

        try
        {
            await _connection.InvokeAsync("JoinEvent", eventContext.EventId!.Value);
        }
        catch (Exception ex)
        {
            _joinError = ex;
        }
    }

    [When(@"je me connecte au hub temps réel sans cookie de session")]
    public async Task QuandJeMeConnecteAuHubTempsReelSansCookieDeSession()
    {
        _connection = BuildConnection(null);

        try
        {
            await _connection.StartAsync();
        }
        catch (Exception ex)
        {
            _connectError = ex;
        }
    }

    [Then(@"la connexion au groupe temps réel réussit")]
    public void AlorsLaConnexionAuGroupeTempsReelReussit() =>
        Assert.Null(_joinError);

    [Then(@"la connexion au groupe temps réel échoue avec une erreur d'autorisation")]
    public void AlorsLaConnexionAuGroupeTempsReelEchoueAvecUneErreurDautorisation() =>
        Assert.IsType<HubException>(_joinError);

    [Then(@"la connexion au hub temps réel est refusée")]
    public void AlorsLaConnexionAuHubTempsReelEstRefusee() =>
        Assert.NotNull(_connectError);

    // TestServer ne peut pas ouvrir de vrai socket TCP : on branche le client SignalR sur le
    // WebSocketClient in-memory de TestServer, et on saute la négociation HTTP habituelle (pas de
    // transport de repli à choisir hors WebSockets dans ce contexte de test).
    private static HubConnection BuildConnection(string? cookieHeader)
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

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }
    }
}
