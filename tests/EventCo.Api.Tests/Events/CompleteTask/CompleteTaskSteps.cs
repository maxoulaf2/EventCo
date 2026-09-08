using EventCo.Api.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace EventCo.Api.Tests.Events.CompleteTask;

[Binding]
public sealed class CompleteTaskSteps(SessionContext sessionContext, EventContext eventContext, TaskContext taskContext)
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };

    private HttpResponseMessage? _response;

    [When(@"je marque cette tâche comme faite via l'API")]
    public async Task QuandJeMarqueCetteTacheCommeFaiteViaLapi() =>
        await Marquer(eventContext.EventId!.Value, taskContext.TaskId!.Value, sessionContext.Cookie);

    [When(@"je marque une tâche comme faite sur un événement inexistant via l'API")]
    public async Task QuandJeMarqueUneTacheCommeFaiteSurUnEvenementInexistantViaLapi() =>
        await Marquer(Guid.NewGuid(), Guid.NewGuid(), sessionContext.Cookie);

    [When(@"je marque une tâche comme faite sur un événement inexistant via l'API sans cookie de session")]
    public async Task QuandJeMarqueUneTacheCommeFaiteSurUnEvenementInexistantViaLapiSansCookieDeSession() =>
        await Marquer(Guid.NewGuid(), Guid.NewGuid(), null);

    private async Task Marquer(Guid eventId, Guid taskId, string? cookieHeader)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/events/{eventId}/tasks/{taskId}/complete");
        if (cookieHeader is not null)
        {
            httpRequest.Headers.Add("Cookie", cookieHeader);
        }

        _response = await client.SendAsync(httpRequest);
    }

    [Then(@"la réponse de marquage a le statut (\d+)")]
    public void AlorsLaReponseDeMarquageALeStatut(int expectedStatusCode) =>
        Assert.Equal(expectedStatusCode, (int)_response!.StatusCode);
}
