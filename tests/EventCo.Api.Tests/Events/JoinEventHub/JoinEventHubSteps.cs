using EventCo.Api.Tests.Support;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Reqnroll;

namespace EventCo.Api.Tests.Events.JoinEventHub;

[Binding]
public sealed class JoinEventHubSteps(SessionContext sessionContext, EventContext eventContext, RealtimeHubConnectionContext hubConnectionContext)
{
    private Exception? _joinError;
    private Exception? _connectError;

    [Given(@"je me connecte au hub temps réel et que je rejoins cet événement")]
    [When(@"je me connecte au hub temps réel et que je rejoins cet événement")]
    public async Task QuandJeMeConnecteAuHubTempsReelEtQueJeRejoinsCetEvenement()
    {
        hubConnectionContext.Connection = HubConnectionTestFactory.Build(sessionContext.Cookie);
        await hubConnectionContext.Connection.StartAsync();

        try
        {
            await hubConnectionContext.Connection.InvokeAsync("JoinEvent", eventContext.EventId!.Value);
        }
        catch (Exception ex)
        {
            _joinError = ex;
        }
    }

    [When(@"je me connecte au hub temps réel sans cookie de session")]
    public async Task QuandJeMeConnecteAuHubTempsReelSansCookieDeSession()
    {
        hubConnectionContext.Connection = HubConnectionTestFactory.Build(null);

        try
        {
            await hubConnectionContext.Connection.StartAsync();
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
}
