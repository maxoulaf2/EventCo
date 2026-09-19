using System.Net.Http.Json;
using EventCo.Api.Contracts.Events;
using EventCo.Api.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace EventCo.Api.Tests.Events.GetEventInvitePreviewByToken;

[Binding]
public sealed class GetEventInvitePreviewByTokenSteps(EventContext eventContext)
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };

    private HttpResponseMessage? _response;
    private EventInvitePreviewResponse? _body;

    [When(@"je consulte l'aperçu de cet événement via l'API sans cookie de session")]
    public async Task QuandJeConsulteLapercuDeCetEvenementViaLapiSansCookieDeSession() =>
        await ConsulterApercu(eventContext.InviteLinkToken!);

    [When(@"je consulte l'aperçu d'un lien d'invitation inexistant via l'API")]
    public async Task QuandJeConsulteLapercuDunLienDinvitationInexistantViaLapi() =>
        await ConsulterApercu("token-qui-n-existe-pas");

    private async Task ConsulterApercu(string token)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);

        _response = await client.GetAsync($"/api/events/invite-links/{token}/preview");
        if (_response.IsSuccessStatusCode)
        {
            _body = await _response.Content.ReadFromJsonAsync<EventInvitePreviewResponse>();
        }
    }

    [Then(@"la réponse d'aperçu a le statut (\d+)")]
    public void AlorsLaReponseDapercuALeStatut(int expectedStatusCode) =>
        Assert.Equal(expectedStatusCode, (int)_response!.StatusCode);

    [Then(@"l'aperçu indique le titre ""(.*)""")]
    public void AlorsLapercuIndiqueLeTitre(string title) =>
        Assert.Equal(title, _body!.Title);
}
