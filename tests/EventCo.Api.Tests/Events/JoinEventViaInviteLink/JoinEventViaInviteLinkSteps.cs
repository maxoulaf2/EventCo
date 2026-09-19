using EventCo.Api.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace EventCo.Api.Tests.Events.JoinEventViaInviteLink;

[Binding]
public sealed class JoinEventViaInviteLinkSteps(SessionContext sessionContext, EventContext eventContext)
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };

    private HttpResponseMessage? _response;

    [When(@"je rejoins cet événement via son lien d'invitation via l'API")]
    public async Task QuandJeRejoinsCetEvenementViaSonLienDinvitationViaLapi() =>
        await Rejoindre(eventContext.InviteLinkToken!, sessionContext.Cookie);

    [When(@"je rejoins un événement via un lien d'invitation inexistant via l'API")]
    public async Task QuandJeRejoinsUnEvenementViaUnLienDinvitationInexistantViaLapi() =>
        await Rejoindre("token-qui-n-existe-pas", sessionContext.Cookie);

    [When(@"je rejoins cet événement via son lien d'invitation via l'API sans cookie de session")]
    public async Task QuandJeRejoinsCetEvenementViaSonLienDinvitationViaLapiSansCookieDeSession() =>
        await Rejoindre(eventContext.InviteLinkToken!, null);

    private async Task Rejoindre(string token, string? cookieHeader)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/events/invite-links/{token}/join");
        if (cookieHeader is not null)
        {
            httpRequest.Headers.Add("Cookie", cookieHeader);
        }

        _response = await client.SendAsync(httpRequest);
    }

    [Then(@"la réponse de participation via lien a le statut (\d+)")]
    public void AlorsLaReponseDeParticipationViaLienALeStatut(int expectedStatusCode) =>
        Assert.Equal(expectedStatusCode, (int)_response!.StatusCode);
}
