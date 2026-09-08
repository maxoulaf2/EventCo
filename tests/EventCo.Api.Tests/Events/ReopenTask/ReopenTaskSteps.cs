using EventCo.Api.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace EventCo.Api.Tests.Events.ReopenTask;

[Binding]
public sealed class ReopenTaskSteps(SessionContext sessionContext, EventContext eventContext, TaskContext taskContext)
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };

    private HttpResponseMessage? _response;

    [Given(@"cette tâche déjà marquée comme faite via l'API")]
    public async Task EtantDonneCetteTacheDejaMarqueeCommeFaiteViaLapi()
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/events/{eventContext.EventId}/tasks/{taskContext.TaskId}/complete");
        httpRequest.Headers.Add("Cookie", sessionContext.Cookie!);

        await client.SendAsync(httpRequest);
    }

    [When(@"je marque cette tâche comme non faite via l'API")]
    public async Task QuandJeMarqueCetteTacheCommeNonFaiteViaLapi() =>
        await Rouvrir(eventContext.EventId!.Value, taskContext.TaskId!.Value, sessionContext.Cookie);

    [When(@"je marque une tâche comme non faite sur un événement inexistant via l'API")]
    public async Task QuandJeMarqueUneTacheCommeNonFaiteSurUnEvenementInexistantViaLapi() =>
        await Rouvrir(Guid.NewGuid(), Guid.NewGuid(), sessionContext.Cookie);

    [When(@"je marque une tâche comme non faite sur un événement inexistant via l'API sans cookie de session")]
    public async Task QuandJeMarqueUneTacheCommeNonFaiteSurUnEvenementInexistantViaLapiSansCookieDeSession() =>
        await Rouvrir(Guid.NewGuid(), Guid.NewGuid(), null);

    private async Task Rouvrir(Guid eventId, Guid taskId, string? cookieHeader)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/events/{eventId}/tasks/{taskId}/reopen");
        if (cookieHeader is not null)
        {
            httpRequest.Headers.Add("Cookie", cookieHeader);
        }

        _response = await client.SendAsync(httpRequest);
    }

    [Then(@"la réponse de réouverture a le statut (\d+)")]
    public void AlorsLaReponseDeReouvertureALeStatut(int expectedStatusCode) =>
        Assert.Equal(expectedStatusCode, (int)_response!.StatusCode);
}
