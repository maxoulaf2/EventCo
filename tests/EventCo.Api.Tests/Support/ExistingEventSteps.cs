using System.Net.Http.Json;
using EventCo.Api.Contracts.Events;
using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace EventCo.Api.Tests.Support;

[Binding]
public sealed class ExistingEventSteps(SessionContext sessionContext, EventContext eventContext)
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };

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
        eventContext.EventId = createdEvent!.Id;
    }
}
