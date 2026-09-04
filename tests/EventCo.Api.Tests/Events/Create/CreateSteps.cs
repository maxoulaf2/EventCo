using System.Net.Http.Json;
using EventCo.Api.Contracts.Events;
using EventCo.Api.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace EventCo.Api.Tests.Events.Create;

[Binding]
public sealed class CreateSteps(SessionContext sessionContext)
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };

    private HttpResponseMessage? _response;
    private EventResponse? _createdEvent;

    [When(@"je crée l'événement ""(.*)""")]
    public async Task CreerEvenement(string title)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);
        var request = new CreateEventRequest(title, "Chez Alice", new DateTime(2026, 12, 24, 0, 0, 0, DateTimeKind.Utc), "Chez Alice");

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/events")
        {
            Content = JsonContent.Create(request),
        };
        if (sessionContext.Cookie is not null)
        {
            httpRequest.Headers.Add("Cookie", sessionContext.Cookie);
        }

        _response = await client.SendAsync(httpRequest);
        if (_response.IsSuccessStatusCode)
        {
            _createdEvent = await _response.Content.ReadFromJsonAsync<EventResponse>();
        }
    }

    [Then(@"la réponse de création d'événement a le statut (\d+)")]
    public void AlorsLaReponseDeCreationDevenementALeStatut(int expectedStatusCode) =>
        Assert.Equal(expectedStatusCode, (int)_response!.StatusCode);

    [Then(@"l'événement créé retourné a pour titre ""(.*)""")]
    public void AlorsLevenementCreeRetourneAPourTitre(string title) =>
        Assert.Equal(title, _createdEvent!.Title);
}
