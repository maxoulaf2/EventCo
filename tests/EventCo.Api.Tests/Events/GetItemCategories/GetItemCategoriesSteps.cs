using System.Net.Http.Json;
using EventCo.Api.Contracts.Events;
using EventCo.Api.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace EventCo.Api.Tests.Events.GetItemCategories;

[Binding]
public sealed class GetItemCategoriesSteps(SessionContext sessionContext)
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };

    private HttpResponseMessage? _response;
    private List<ItemCategoryResponse>? _categories;

    [When(@"je consulte les catégories d'articles via l'API")]
    public async Task QuandJeConsulteLesCategoriesDarticlesViaLapi() => await Consulter(sessionContext.Cookie);

    [When(@"je consulte les catégories d'articles via l'API sans cookie de session")]
    public async Task QuandJeConsulteLesCategoriesDarticlesViaLapiSansCookieDeSession() => await Consulter(null);

    private async Task Consulter(string? cookieHeader)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/item-categories");
        if (cookieHeader is not null)
        {
            request.Headers.Add("Cookie", cookieHeader);
        }

        _response = await client.SendAsync(request);
        if (_response.IsSuccessStatusCode)
        {
            _categories = await _response.Content.ReadFromJsonAsync<List<ItemCategoryResponse>>();
        }
    }

    [Then(@"la réponse de consultation des catégories a le statut (\d+)")]
    public void AlorsLaReponseDeConsultationDesCategoriesALeStatut(int expectedStatusCode) =>
        Assert.Equal(expectedStatusCode, (int)_response!.StatusCode);

    [Then(@"les catégories retournées sont dans l'ordre ""(.*)""")]
    public void AlorsLesCategoriesRetourneesSontDansLordre(string labels) =>
        Assert.Equal(labels.Split("\", \""), _categories!.Select(c => c.Label));

    [Then(@"la catégorie retournée ""(.*)"" a la clé ""(.*)""")]
    public void AlorsLaCategorieRetourneeALaCle(string label, string key) =>
        Assert.Equal(key, _categories!.Single(c => c.Label == label).Key);
}
