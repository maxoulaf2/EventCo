using System.Net.Http.Json;
using EventCo.Api.Contracts.Events;
using EventCo.Api.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace EventCo.Api.Tests.Events.GetEventItems;

[Binding]
public sealed class GetEventItemsSteps(SessionContext sessionContext, EventContext eventContext)
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };

    private HttpResponseMessage? _response;
    private List<EventItemResponse>? _consultedItems;

    [When(@"je consulte les articles de cet événement via l'API")]
    public async Task QuandJeConsulteLesArticlesDeCetEvenementViaLapi() =>
        await AppelerAvecCookie(eventContext.EventId!.Value, sessionContext.Cookie);

    [When(@"je consulte les articles d'un événement inexistant via l'API")]
    public async Task QuandJeConsulteLesArticlesDunEvenementInexistantViaLapi() =>
        await AppelerAvecCookie(Guid.NewGuid(), sessionContext.Cookie);

    [When(@"je consulte les articles d'un événement inexistant via l'API sans cookie de session")]
    public async Task QuandJeConsulteLesArticlesDunEvenementInexistantViaLapiSansCookieDeSession() =>
        await AppelerAvecCookie(Guid.NewGuid(), null);

    private async Task AppelerAvecCookie(Guid eventId, string? cookieHeader)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/events/{eventId}/items");
        if (cookieHeader is not null)
        {
            request.Headers.Add("Cookie", cookieHeader);
        }

        _response = await client.SendAsync(request);
        if (_response.IsSuccessStatusCode)
        {
            _consultedItems = await _response.Content.ReadFromJsonAsync<List<EventItemResponse>>();
        }
    }

    [Then(@"la réponse de consultation des articles a le statut (\d+)")]
    public void AlorsLaReponseDeConsultationDesArticlesALeStatut(int expectedStatusCode) =>
        Assert.Equal(expectedStatusCode, (int)_response!.StatusCode);

    [Then(@"les articles consultés retournés contiennent (\d+) articles?")]
    public void AlorsLesArticlesConsultesRetournesContiennentArticles(int count) =>
        Assert.Equal(count, _consultedItems!.Count);

    [Then(@"les articles consultés retournés contiennent un article ""(.*)""")]
    public void AlorsLesArticlesConsultesRetournesContiennentUnArticle(string title) =>
        Assert.Single(_consultedItems!, t => t.Title == title);
}
