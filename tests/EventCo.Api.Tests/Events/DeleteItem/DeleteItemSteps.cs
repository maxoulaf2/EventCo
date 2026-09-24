using EventCo.Api.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace EventCo.Api.Tests.Events.DeleteItem;

[Binding]
public sealed class DeleteItemSteps(SessionContext sessionContext, EventContext eventContext, ItemContext itemContext)
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };

    private HttpResponseMessage? _response;

    [When(@"je supprime cet article via l'API")]
    public async Task QuandJeSupprimeCetArticleViaLapi() =>
        await Supprimer(eventContext.EventId!.Value, itemContext.ItemId!.Value, sessionContext.Cookie);

    [When(@"je supprime un article sur un événement inexistant via l'API")]
    public async Task QuandJeSupprimeUnArticleSurUnEvenementInexistantViaLapi() =>
        await Supprimer(Guid.NewGuid(), Guid.NewGuid(), sessionContext.Cookie);

    [When(@"je supprime un article sur un événement inexistant via l'API sans cookie de session")]
    public async Task QuandJeSupprimeUnArticleSurUnEvenementInexistantViaLapiSansCookieDeSession() =>
        await Supprimer(Guid.NewGuid(), Guid.NewGuid(), null);

    [When(@"je supprime un article inexistant sur cet événement via l'API")]
    public async Task QuandJeSupprimeUnArticleInexistantSurCetEvenementViaLapi() =>
        await Supprimer(eventContext.EventId!.Value, Guid.NewGuid(), sessionContext.Cookie);

    private async Task Supprimer(Guid eventId, Guid itemId, string? cookieHeader)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/events/{eventId}/items/{itemId}");
        if (cookieHeader is not null)
        {
            httpRequest.Headers.Add("Cookie", cookieHeader);
        }

        _response = await client.SendAsync(httpRequest);
    }

    [Then(@"la réponse de suppression d'article a le statut (\d+)")]
    public void AlorsLaReponseDeSuppressionDarticleALeStatut(int expectedStatusCode) =>
        Assert.Equal(expectedStatusCode, (int)_response!.StatusCode);
}
