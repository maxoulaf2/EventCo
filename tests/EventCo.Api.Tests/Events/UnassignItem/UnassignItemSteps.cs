using EventCo.Api.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace EventCo.Api.Tests.Events.UnassignItem;

[Binding]
public sealed class UnassignItemSteps(SessionContext sessionContext, EventContext eventContext, ItemContext itemContext)
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };

    private HttpResponseMessage? _response;

    [When(@"je désassigne cet article via l'API")]
    public async Task QuandJeDesassigneCetArticleViaLapi() =>
        await Desassigner(eventContext.EventId!.Value, itemContext.ItemId!.Value, sessionContext.Cookie);

    [When(@"je désassigne un article sur un événement inexistant via l'API")]
    public async Task QuandJeDesassigneUnArticleSurUnEvenementInexistantViaLapi() =>
        await Desassigner(Guid.NewGuid(), Guid.NewGuid(), sessionContext.Cookie);

    [When(@"je désassigne un article sur un événement inexistant via l'API sans cookie de session")]
    public async Task QuandJeDesassigneUnArticleSurUnEvenementInexistantViaLapiSansCookieDeSession() =>
        await Desassigner(Guid.NewGuid(), Guid.NewGuid(), null);

    private async Task Desassigner(Guid eventId, Guid itemId, string? cookieHeader)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/events/{eventId}/items/{itemId}/unassign");
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
