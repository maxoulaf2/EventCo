using System.Net.Http.Json;
using EventCo.Api.Contracts.Events;
using EventCo.Api.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace EventCo.Api.Tests.Events.RegenerateEventInviteLink;

[Binding]
public sealed class RegenerateEventInviteLinkSteps(SessionContext sessionContext, EventContext eventContext)
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };

    private HttpResponseMessage? _response;
    private RegenerateEventInviteLinkResponse? _body;

    [When(@"je régénère le lien d'invitation de cet événement via l'API")]
    public async Task QuandJeRegenereLeLienDinvitationDeCetEvenementViaLapi() =>
        await Regenerer(eventContext.EventId!.Value, sessionContext.Cookie);

    [When(@"je régénère le lien d'invitation de cet événement via l'API sans cookie de session")]
    public async Task QuandJeRegenereLeLienDinvitationDeCetEvenementViaLapiSansCookieDeSession() =>
        await Regenerer(eventContext.EventId!.Value, null);

    [When(@"je régénère le lien d'invitation d'un événement inexistant via l'API")]
    public async Task QuandJeRegenereLeLienDinvitationDunEvenementInexistantViaLapi() =>
        await Regenerer(Guid.NewGuid(), sessionContext.Cookie);

    private async Task Regenerer(Guid eventId, string? cookieHeader)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/events/{eventId}/invite-link/regenerate");
        if (cookieHeader is not null)
        {
            httpRequest.Headers.Add("Cookie", cookieHeader);
        }

        _response = await client.SendAsync(httpRequest);
        if (_response.IsSuccessStatusCode)
        {
            _body = await _response.Content.ReadFromJsonAsync<RegenerateEventInviteLinkResponse>();
        }
    }

    [Then(@"la réponse de régénération a le statut (\d+)")]
    public void AlorsLaReponseDeRegenerationALeStatut(int expectedStatusCode) =>
        Assert.Equal(expectedStatusCode, (int)_response!.StatusCode);

    [Then(@"le nouveau lien d'invitation diffère de l'ancien")]
    public void AlorsLeNouveauLienDinvitationDiffereDeLancien() =>
        Assert.NotEqual(eventContext.InviteLinkToken, _body!.InviteLinkToken);
}
