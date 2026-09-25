using System.Net.Http.Json;
using EventCo.Api.Contracts.Events;
using EventCo.Api.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace EventCo.Api.Tests.Events.CreateItem;

[Binding]
public sealed class CreateItemSteps(SessionContext sessionContext, EventContext eventContext, InvitedParticipantContext invitedParticipantContext)
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };

    private HttpResponseMessage? _response;
    private EventItemResponse? _createdItem;

    [When(@"j'ajoute l'article à prendre ""(.*)"" de quantité ""(.*)"" à cet événement via l'API")]
    public async Task QuandJajouteLarticleAPrendreACetEvenementViaLapi(string title, string quantity) =>
        await AjouterArticle(eventContext.EventId!.Value, title, quantity, "ToBring", sessionContext.Cookie);

    [When(@"j'ajoute l'article que j'apporte ""(.*)"" de quantité ""(.*)"" à cet événement via l'API")]
    public async Task QuandJajouteLarticleQueJapporteACetEvenementViaLapi(string title, string quantity) =>
        await AjouterArticle(eventContext.EventId!.Value, title, quantity, "Contribution", sessionContext.Cookie);

    [When(@"j'ajoute l'article de nature ""(.*)"" ""(.*)"" de quantité ""(.*)"" à cet événement via l'API")]
    public async Task QuandJajouteLarticleDeNatureACetEvenementViaLapi(string kind, string title, string quantity) =>
        await AjouterArticle(eventContext.EventId!.Value, title, quantity, kind, sessionContext.Cookie);

    [When(@"j'ajoute l'article que j'apporte ""(.*)"" de quantité ""(.*)"" à un événement inexistant via l'API")]
    public async Task QuandJajouteLarticleQueJapporteAUnEvenementInexistantViaLapi(string title, string quantity) =>
        await AjouterArticle(Guid.NewGuid(), title, quantity, "Contribution", sessionContext.Cookie);

    [When(@"j'ajoute l'article que j'apporte ""(.*)"" de quantité ""(.*)"" à un événement inexistant via l'API sans cookie de session")]
    public async Task QuandJajouteLarticleQueJapporteAUnEvenementInexistantViaLapiSansCookieDeSession(string title, string quantity) =>
        await AjouterArticle(Guid.NewGuid(), title, quantity, "Contribution", null);

    private async Task AjouterArticle(Guid eventId, string title, string quantity, string kind, string? cookieHeader)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/events/{eventId}/items")
        {
            Content = JsonContent.Create(new CreateItemRequest(title, quantity, kind)),
        };
        if (cookieHeader is not null)
        {
            httpRequest.Headers.Add("Cookie", cookieHeader);
        }

        _response = await client.SendAsync(httpRequest);
    }

    // Le corps de la réponse ne peut être lu qu'une fois, alors que plusieurs steps Then le consultent.
    private async Task<EventItemResponse?> LireArticleCree() =>
        _createdItem ??= await _response!.Content.ReadFromJsonAsync<EventItemResponse>();

    [Then(@"la réponse de création d'article a le statut (\d+)")]
    public void AlorsLaReponseDeCreationDarticleALeStatut(int expectedStatusCode) =>
        Assert.Equal(expectedStatusCode, (int)_response!.StatusCode);

    [Then(@"l'article créé via l'API est de nature ""(.*)""")]
    public async Task AlorsLarticleCreeViaLapiEstDeNature(string kind)
    {
        var item = await LireArticleCree();
        Assert.Equal(kind, item!.Kind);
    }

    [Then(@"l'article créé via l'API n'est attribué à personne")]
    public async Task AlorsLarticleCreeViaLapiNestAttribueAPersonne()
    {
        var item = await LireArticleCree();
        Assert.Null(item!.AssignedToUserId);
    }

    [Then(@"l'article créé via l'API est attribué à ce participant")]
    public async Task AlorsLarticleCreeViaLapiEstAttribueACeParticipant()
    {
        var item = await LireArticleCree();
        Assert.Equal(invitedParticipantContext.UserId, item!.AssignedToUserId);
    }
}
