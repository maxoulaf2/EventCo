using System.Net.Http.Json;
using EventCo.Api.Contracts.Events;
using EventCo.Api.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace EventCo.Api.Tests.Events.GetById;

[Binding]
public sealed class GetByIdSteps(SessionContext sessionContext)
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };

    private Guid? _existingEventId;
    private HttpResponseMessage? _response;
    private EventResponse? _consultedEvent;

    [Given(@"un événement ""(.*)"" créé via l'API")]
    public async Task EtantDonneUnEvenementCreeViaLapi(string title)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);
        var request = new CreateEventRequest(title, "Chez Alice", new DateTime(2026, 12, 24, 0, 0, 0, DateTimeKind.Utc), "Chez Alice");

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/events")
        {
            Content = JsonContent.Create(request),
        };
        httpRequest.Headers.Add("Cookie", sessionContext.Cookie!);

        var response = await client.SendAsync(httpRequest);
        var createdEvent = await response.Content.ReadFromJsonAsync<EventResponse>();
        _existingEventId = createdEvent!.Id;
    }

    [When(@"je consulte cet événement via l'API")]
    public async Task QuandJeConsulteCetEvenementViaLapi() =>
        await AppelerAvecCookie(_existingEventId!.Value, sessionContext.Cookie);

    [When(@"je consulte un événement inexistant via l'API")]
    public async Task QuandJeConsulteUnEvenementInexistantViaLapi() =>
        await AppelerAvecCookie(Guid.NewGuid(), sessionContext.Cookie);

    [When(@"je consulte un événement inexistant via l'API sans cookie de session")]
    public async Task QuandJeConsulteUnEvenementInexistantViaLapiSansCookieDeSession() =>
        await AppelerAvecCookie(Guid.NewGuid(), null);

    private async Task AppelerAvecCookie(Guid eventId, string? cookieHeader)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/events/{eventId}");
        if (cookieHeader is not null)
        {
            request.Headers.Add("Cookie", cookieHeader);
        }

        _response = await client.SendAsync(request);
        if (_response.IsSuccessStatusCode)
        {
            _consultedEvent = await _response.Content.ReadFromJsonAsync<EventResponse>();
        }
    }

    [Then(@"la réponse de consultation d'événement a le statut (\d+)")]
    public void AlorsLaReponseDeConsultationDevenementALeStatut(int expectedStatusCode) =>
        Assert.Equal(expectedStatusCode, (int)_response!.StatusCode);

    [Then(@"l'événement consulté retourné a pour titre ""(.*)""")]
    public void AlorsLevenementConsulteRetourneAPourTitre(string title) =>
        Assert.Equal(title, _consultedEvent!.Title);
}
