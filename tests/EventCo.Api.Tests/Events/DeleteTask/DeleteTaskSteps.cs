using EventCo.Api.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace EventCo.Api.Tests.Events.DeleteTask;

[Binding]
public sealed class DeleteTaskSteps(SessionContext sessionContext, EventContext eventContext, TaskContext taskContext)
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };

    private HttpResponseMessage? _response;

    [When(@"je supprime cette tâche via l'API")]
    public async Task QuandJeSupprimeCetteTacheViaLapi() =>
        await Supprimer(eventContext.EventId!.Value, taskContext.TaskId!.Value, sessionContext.Cookie);

    [When(@"je supprime une tâche sur un événement inexistant via l'API")]
    public async Task QuandJeSupprimeUneTacheSurUnEvenementInexistantViaLapi() =>
        await Supprimer(Guid.NewGuid(), Guid.NewGuid(), sessionContext.Cookie);

    [When(@"je supprime une tâche sur un événement inexistant via l'API sans cookie de session")]
    public async Task QuandJeSupprimeUneTacheSurUnEvenementInexistantViaLapiSansCookieDeSession() =>
        await Supprimer(Guid.NewGuid(), Guid.NewGuid(), null);

    private async Task Supprimer(Guid eventId, Guid taskId, string? cookieHeader)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/events/{eventId}/tasks/{taskId}");
        if (cookieHeader is not null)
        {
            httpRequest.Headers.Add("Cookie", cookieHeader);
        }

        _response = await client.SendAsync(httpRequest);
    }

    [Then(@"la réponse de suppression de tâche a le statut (\d+)")]
    public void AlorsLaReponseDeSuppressionDeTacheALeStatut(int expectedStatusCode) =>
        Assert.Equal(expectedStatusCode, (int)_response!.StatusCode);
}
