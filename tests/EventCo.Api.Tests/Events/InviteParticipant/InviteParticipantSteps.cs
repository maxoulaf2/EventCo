using System.Net.Http.Json;
using EventCo.Api.Contracts.Events;
using EventCo.Api.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace EventCo.Api.Tests.Events.InviteParticipant;

[Binding]
public sealed class InviteParticipantSteps(SessionContext sessionContext, EventContext eventContext)
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };

    private HttpResponseMessage? _response;

    [When(@"j'invite ""(.*)"" à cet événement via l'API")]
    public async Task QuandJinviteACetEvenementViaLapi(string email) =>
        await Inviter(eventContext.EventId!.Value, email, sessionContext.Cookie);

    [When(@"j'invite ""(.*)"" à un événement inexistant via l'API")]
    public async Task QuandJinviteAUnEvenementInexistantViaLapi(string email) =>
        await Inviter(Guid.NewGuid(), email, sessionContext.Cookie);

    [When(@"j'invite ""(.*)"" à un événement inexistant via l'API sans cookie de session")]
    public async Task QuandJinviteAUnEvenementInexistantViaLapiSansCookieDeSession(string email) =>
        await Inviter(Guid.NewGuid(), email, null);

    private async Task Inviter(Guid eventId, string email, string? cookieHeader)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/events/{eventId}/participants")
        {
            Content = JsonContent.Create(new InviteParticipantRequest(email)),
        };
        if (cookieHeader is not null)
        {
            httpRequest.Headers.Add("Cookie", cookieHeader);
        }

        _response = await client.SendAsync(httpRequest);
    }

    [Then(@"la réponse d'invitation a le statut (\d+)")]
    public void AlorsLaReponseDinvitationALeStatut(int expectedStatusCode) =>
        Assert.Equal(expectedStatusCode, (int)_response!.StatusCode);
}
