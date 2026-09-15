using System.Net.Http.Json;
using EventCo.Api.Contracts.Events;
using EventCo.Api.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace EventCo.Api.Tests.Events.GetEventTasks;

[Binding]
public sealed class GetEventTasksSteps(SessionContext sessionContext, EventContext eventContext)
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };

    private HttpResponseMessage? _response;
    private List<EventTaskResponse>? _consultedTasks;

    [When(@"je consulte les tâches de cet événement via l'API")]
    public async Task QuandJeConsulteLesTachesDeCetEvenementViaLapi() =>
        await AppelerAvecCookie(eventContext.EventId!.Value, sessionContext.Cookie);

    [When(@"je consulte les tâches d'un événement inexistant via l'API")]
    public async Task QuandJeConsulteLesTachesDunEvenementInexistantViaLapi() =>
        await AppelerAvecCookie(Guid.NewGuid(), sessionContext.Cookie);

    [When(@"je consulte les tâches d'un événement inexistant via l'API sans cookie de session")]
    public async Task QuandJeConsulteLesTachesDunEvenementInexistantViaLapiSansCookieDeSession() =>
        await AppelerAvecCookie(Guid.NewGuid(), null);

    private async Task AppelerAvecCookie(Guid eventId, string? cookieHeader)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/events/{eventId}/tasks");
        if (cookieHeader is not null)
        {
            request.Headers.Add("Cookie", cookieHeader);
        }

        _response = await client.SendAsync(request);
        if (_response.IsSuccessStatusCode)
        {
            _consultedTasks = await _response.Content.ReadFromJsonAsync<List<EventTaskResponse>>();
        }
    }

    [Then(@"la réponse de consultation des tâches a le statut (\d+)")]
    public void AlorsLaReponseDeConsultationDesTachesALeStatut(int expectedStatusCode) =>
        Assert.Equal(expectedStatusCode, (int)_response!.StatusCode);

    [Then(@"les tâches consultées retournées contiennent (\d+) tâches?")]
    public void AlorsLesTachesConsulteesRetourneesContiennentTaches(int count) =>
        Assert.Equal(count, _consultedTasks!.Count);

    [Then(@"les tâches consultées retournées contiennent une tâche ""(.*)"" non faite")]
    public void AlorsLesTachesConsulteesRetourneesContiennentUneTacheNonFaite(string title)
    {
        var task = Assert.Single(_consultedTasks!, t => t.Title == title);
        Assert.False(task.IsDone);
    }
}
