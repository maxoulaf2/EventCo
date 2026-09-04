using System.Net.Http.Json;
using EventCo.Api.Contracts.Events;
using EventCo.Api.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace EventCo.Api.Tests.Events.Update;

[Binding]
public sealed class UpdateSteps(SessionContext sessionContext, EventContext eventContext)
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };

    private HttpResponseMessage? _response;
    private EventResponse? _updatedEvent;

    [When(@"je modifie cet événement via l'API avec le titre ""(.*)"" au lieu ""(.*)""")]
    public async Task QuandJeModifieCetEvenementViaLapiAvecLeTitreAuLieu(string title, string location) =>
        await AppelerAvecCookie(eventContext.EventId!.Value, title, location, sessionContext.Cookie);

    [When(@"je modifie un événement inexistant via l'API avec le titre ""(.*)"" au lieu ""(.*)""")]
    public async Task QuandJeModifieUnEvenementInexistantViaLapiAvecLeTitreAuLieu(string title, string location) =>
        await AppelerAvecCookie(Guid.NewGuid(), title, location, sessionContext.Cookie);

    [When(@"je modifie un événement inexistant via l'API avec le titre ""(.*)"" au lieu ""(.*)"" sans cookie de session")]
    public async Task QuandJeModifieUnEvenementInexistantViaLapiAvecLeTitreAuLieuSansCookieDeSession(string title, string location) =>
        await AppelerAvecCookie(Guid.NewGuid(), title, location, null);

    private async Task AppelerAvecCookie(Guid eventId, string title, string location, string? cookieHeader)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);
        var request = new UpdateEventRequest(title, "Chez Alice", new DateTime(2026, 12, 25, 0, 0, 0, DateTimeKind.Utc), location);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/events/{eventId}")
        {
            Content = JsonContent.Create(request),
        };
        if (cookieHeader is not null)
        {
            httpRequest.Headers.Add("Cookie", cookieHeader);
        }

        _response = await client.SendAsync(httpRequest);
        if (_response.IsSuccessStatusCode)
        {
            _updatedEvent = await _response.Content.ReadFromJsonAsync<EventResponse>();
        }
    }

    [Then(@"la réponse de modification d'événement a le statut (\d+)")]
    public void AlorsLaReponseDeModificationDevenementALeStatut(int expectedStatusCode) =>
        Assert.Equal(expectedStatusCode, (int)_response!.StatusCode);

    [Then(@"l'événement modifié retourné a pour titre ""(.*)""")]
    public void AlorsLevenementModifieRetourneAPourTitre(string title) =>
        Assert.Equal(title, _updatedEvent!.Title);
}
