using EventCo.Api.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace EventCo.Api.Tests.Events.UnassignTask;

[Binding]
public sealed class UnassignTaskSteps(SessionContext sessionContext, EventContext eventContext, TaskContext taskContext)
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };

    private HttpResponseMessage? _response;

    [When(@"je désassigne cette tâche via l'API")]
    public async Task QuandJeDesassigneCetteTacheViaLapi() =>
        await Desassigner(eventContext.EventId!.Value, taskContext.TaskId!.Value, sessionContext.Cookie);

    [When(@"je désassigne une tâche sur un événement inexistant via l'API")]
    public async Task QuandJeDesassigneUneTacheSurUnEvenementInexistantViaLapi() =>
        await Desassigner(Guid.NewGuid(), Guid.NewGuid(), sessionContext.Cookie);

    [When(@"je désassigne une tâche sur un événement inexistant via l'API sans cookie de session")]
    public async Task QuandJeDesassigneUneTacheSurUnEvenementInexistantViaLapiSansCookieDeSession() =>
        await Desassigner(Guid.NewGuid(), Guid.NewGuid(), null);

    private async Task Desassigner(Guid eventId, Guid taskId, string? cookieHeader)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/events/{eventId}/tasks/{taskId}/unassign");
        if (cookieHeader is not null)
        {
            httpRequest.Headers.Add("Cookie", cookieHeader);
        }

        _response = await client.SendAsync(httpRequest);
    }

    [Then(@"la réponse de désassignation a le statut (\d+)")]
    public void AlorsLaReponseDeDesassignationALeStatut(int expectedStatusCode) =>
        Assert.Equal(expectedStatusCode, (int)_response!.StatusCode);
}
