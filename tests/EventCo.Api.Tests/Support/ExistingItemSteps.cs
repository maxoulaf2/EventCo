using System.Net.Http.Json;
using EventCo.Api.Contracts.Events;
using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace EventCo.Api.Tests.Support;

[Binding]
public sealed class ExistingItemSteps(SessionContext sessionContext, EventContext eventContext, ItemContext itemContext)
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };

    [Given(@"un article à prendre ""(.*)"" ajouté à cet événement via l'API")]
    public async Task EtantDonneUnArticleAPrendreAjouteACetEvenementViaLapi(string title) =>
        await AjouterArticle(title, "ToBring");

    [Given(@"un article apporté ""(.*)"" ajouté à cet événement via l'API")]
    public async Task EtantDonneUnArticleApporteAjouteACetEvenementViaLapi(string title) =>
        await AjouterArticle(title, "Contribution");

    private async Task AjouterArticle(string title, string kind)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/events/{eventContext.EventId}/items")
        {
            Content = JsonContent.Create(new CreateItemRequest(title, "1", kind)),
        };
        httpRequest.Headers.Add("Cookie", sessionContext.Cookie!);

        var response = await client.SendAsync(httpRequest);
        var item = await response.Content.ReadFromJsonAsync<EventItemResponse>();
        itemContext.ItemId = item!.Id;
    }
}
