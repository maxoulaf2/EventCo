using System.Net.Http.Json;
using EventCo.Api.Contracts.Events;
using EventCo.Api.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace EventCo.Api.Tests.Events.CorrectItemCategory;

[Binding]
public sealed class CorrectItemCategorySteps(SessionContext sessionContext, EventContext eventContext, ItemContext itemContext)
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };

    private HttpResponseMessage? _response;

    [When(@"je corrige la catégorie de cet article en ""(.*)"" via l'API")]
    public async Task QuandJeCorrigeLaCategorieDeCetArticleEnViaLapi(string category) =>
        await Corriger(eventContext.EventId!.Value, itemContext.ItemId!.Value, category, sessionContext.Cookie);

    [When(@"je corrige la catégorie d'un article sur un événement inexistant via l'API")]
    public async Task QuandJeCorrigeLaCategorieDunArticleSurUnEvenementInexistantViaLapi() =>
        await Corriger(Guid.NewGuid(), Guid.NewGuid(), "Desserts", sessionContext.Cookie);

    [When(@"je corrige la catégorie d'un article sur un événement inexistant via l'API sans cookie de session")]
    public async Task QuandJeCorrigeLaCategorieDunArticleSurUnEvenementInexistantViaLapiSansCookieDeSession() =>
        await Corriger(Guid.NewGuid(), Guid.NewGuid(), "Desserts", null);

    private async Task Corriger(Guid eventId, Guid itemId, string category, string? cookieHeader)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/events/{eventId}/items/{itemId}/category")
        {
            Content = JsonContent.Create(new CorrectItemCategoryRequest(category)),
        };
        if (cookieHeader is not null)
        {
            httpRequest.Headers.Add("Cookie", cookieHeader);
        }

        _response = await client.SendAsync(httpRequest);
    }

    [Then(@"la réponse de correction de catégorie a le statut (\d+)")]
    public void AlorsLaReponseDeCorrectionDeCategorieALeStatut(int expectedStatusCode) =>
        Assert.Equal(expectedStatusCode, (int)_response!.StatusCode);
}
