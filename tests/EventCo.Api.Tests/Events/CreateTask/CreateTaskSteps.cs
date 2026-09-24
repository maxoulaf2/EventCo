using System.Net.Http.Json;
using EventCo.Api.Contracts.Events;
using EventCo.Api.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace EventCo.Api.Tests.Events.CreateTask;

[Binding]
public sealed class CreateTaskSteps(SessionContext sessionContext, EventContext eventContext)
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };

    private HttpResponseMessage? _response;

    [When(@"j'ajoute la tâche ""(.*)"" de quantité ""(.*)"" à cet événement via l'API")]
    public async Task QuandJajouteLaTacheACetEvenementViaLapi(string title, string quantity) =>
        await AjouterTache(eventContext.EventId!.Value, title, quantity, sessionContext.Cookie);

    [When(@"j'ajoute la tâche ""(.*)"" de quantité ""(.*)"" à un événement inexistant via l'API")]
    public async Task QuandJajouteLaTacheAUnEvenementInexistantViaLapi(string title, string quantity) =>
        await AjouterTache(Guid.NewGuid(), title, quantity, sessionContext.Cookie);

    [When(@"j'ajoute la tâche ""(.*)"" de quantité ""(.*)"" à un événement inexistant via l'API sans cookie de session")]
    public async Task QuandJajouteLaTacheAUnEvenementInexistantViaLapiSansCookieDeSession(string title, string quantity) =>
        await AjouterTache(Guid.NewGuid(), title, quantity, null);

    private async Task AjouterTache(Guid eventId, string title, string quantity, string? cookieHeader)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/events/{eventId}/tasks")
        {
            Content = JsonContent.Create(new CreateTaskRequest(title, quantity)),
        };
        if (cookieHeader is not null)
        {
            httpRequest.Headers.Add("Cookie", cookieHeader);
        }

        _response = await client.SendAsync(httpRequest);
    }

    [Then(@"la réponse de création de tâche a le statut (\d+)")]
    public void AlorsLaReponseDeCreationDeTacheALeStatut(int expectedStatusCode) =>
        Assert.Equal(expectedStatusCode, (int)_response!.StatusCode);
}
