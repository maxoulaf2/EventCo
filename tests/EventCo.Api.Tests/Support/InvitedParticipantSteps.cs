using System.Net.Http.Json;
using EventCo.Api.Contracts.Events;
using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace EventCo.Api.Tests.Support;

[Binding]
public sealed class InvitedParticipantSteps(SessionContext sessionContext, EventContext eventContext, InvitedParticipantContext invitedParticipantContext)
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };

    [Given(@"""(.*)"" invité à cet événement via l'API")]
    public async Task EtantDonneInviteACetEvenementViaLapi(string email)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/events/{eventContext.EventId}/participants")
        {
            Content = JsonContent.Create(new InviteParticipantRequest(email)),
        };
        httpRequest.Headers.Add("Cookie", sessionContext.Cookie!);

        var response = await client.SendAsync(httpRequest);
        var participant = await response.Content.ReadFromJsonAsync<EventParticipantResponse>();
        invitedParticipantContext.UserId = participant!.UserId;
    }
}
