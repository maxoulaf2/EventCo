using System.Net.Http.Json;
using EventCo.Api.Contracts.Events;
using EventCo.Api.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace EventCo.Api.Tests.Events.CreateItem;

[Binding]
public sealed class CreateItemSteps(SessionContext sessionContext, EventContext eventContext)
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };

    private HttpResponseMessage? _response;

    [When(@"j'ajoute l'article ""(.*)"" de quantité ""(.*)"" à cet événement via l'API")]
    public async Task QuandJajouteLarticleACetEvenementViaLapi(string title, string quantity) =>
        await AjouterArticle(eventContext.EventId!.Value, title, quantity, sessionContext.Cookie);

    [When(@"j'ajoute l'article ""(.*)"" de quantité ""(.*)"" à un événement inexistant via l'API")]
    public async Task QuandJajouteLarticleAUnEvenementInexistantViaLapi(string title, string quantity) =>
        await AjouterArticle(Guid.NewGuid(), title, quantity, sessionContext.Cookie);

    [When(@"j'ajoute l'article ""(.*)"" de quantité ""(.*)"" à un événement inexistant via l'API sans cookie de session")]
    public async Task QuandJajouteLarticleAUnEvenementInexistantViaLapiSansCookieDeSession(string title, string quantity) =>
        await AjouterArticle(Guid.NewGuid(), title, quantity, null);

    private async Task AjouterArticle(Guid eventId, string title, string quantity, string? cookieHeader)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/events/{eventId}/items")
        {
            Content = JsonContent.Create(new CreateItemRequest(title, quantity)),
        };
        if (cookieHeader is not null)
        {
            httpRequest.Headers.Add("Cookie", cookieHeader);
        }

        _response = await client.SendAsync(httpRequest);
    }

    [Then(@"la réponse de création d'article a le statut (\d+)")]
    public void AlorsLaReponseDeCreationDarticleALeStatut(int expectedStatusCode) =>
        Assert.Equal(expectedStatusCode, (int)_response!.StatusCode);
}
