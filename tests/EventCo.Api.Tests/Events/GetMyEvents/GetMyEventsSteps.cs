using System.Net.Http.Json;
using EventCo.Api.Contracts.Events;
using EventCo.Api.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace EventCo.Api.Tests.Events.GetMyEvents;

[Binding]
public sealed class GetMyEvents(SessionContext sessionContext)
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };

    private HttpResponseMessage? _response;
    private List<EventSummaryResponse>? _myEvents;

    [When(@"je consulte la liste de mes événements via l'API")]
    public async Task QuandJeConsulteLaListeDeMesEvenementsViaLapi() =>
        await AppelerAvecCookie(sessionContext.Cookie);

    [When(@"je consulte la liste de mes événements via l'API sans cookie de session")]
    public async Task QuandJeConsulteLaListeDeMesEvenementsViaLapiSansCookieDeSession() =>
        await AppelerAvecCookie(null);

    private async Task AppelerAvecCookie(string? cookieHeader)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/events");
        if (cookieHeader is not null)
        {
            request.Headers.Add("Cookie", cookieHeader);
        }

        _response = await client.SendAsync(request);
        if (_response.IsSuccessStatusCode)
        {
            _myEvents = await _response.Content.ReadFromJsonAsync<List<EventSummaryResponse>>();
        }
    }

    [Then(@"la réponse de liste d'événements a le statut (\d+)")]
    public void AlorsLaReponseDeListeDevenementsALeStatut(int expectedStatusCode) =>
        Assert.Equal(expectedStatusCode, (int)_response!.StatusCode);

    [Then(@"ma liste d'événements retournée contient ""(.*)""")]
    public void AlorsMaListeDevenementsRetourneeContient(string title) =>
        Assert.Contains(_myEvents!, e => e.Title == title);
}
