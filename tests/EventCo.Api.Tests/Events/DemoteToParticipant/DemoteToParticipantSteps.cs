using EventCo.Api.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace EventCo.Api.Tests.Events.DemoteToParticipant;

[Binding]
public sealed class DemoteToParticipantSteps(SessionContext sessionContext, EventContext eventContext, InvitedParticipantContext invitedParticipantContext)
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };

    private HttpResponseMessage? _response;

    [When(@"je rétrograde ce co-organisateur en participant via l'API")]
    public async Task QuandJeRetrogradeCeCoOrganisateurEnParticipantViaLapi() =>
        await Retrograder(eventContext.EventId!.Value, invitedParticipantContext.UserId!.Value, sessionContext.Cookie);

    [When(@"je rétrograde le créateur de l'événement via l'API")]
    public async Task QuandJeRetrogradeLeCreateurDeLevenementViaLapi() =>
        await Retrograder(eventContext.EventId!.Value, eventContext.CreatedByUserId!.Value, sessionContext.Cookie);

    [When(@"je rétrograde un co-organisateur sur un événement inexistant via l'API")]
    public async Task QuandJeRetrogradeUnCoOrganisateurSurUnEvenementInexistantViaLapi() =>
        await Retrograder(Guid.NewGuid(), Guid.NewGuid(), sessionContext.Cookie);

    [When(@"je rétrograde un co-organisateur sur un événement inexistant via l'API sans cookie de session")]
    public async Task QuandJeRetrogradeUnCoOrganisateurSurUnEvenementInexistantViaLapiSansCookieDeSession() =>
        await Retrograder(Guid.NewGuid(), Guid.NewGuid(), null);

    [When(@"je rétrograde un utilisateur qui ne participe pas à cet événement via l'API")]
    public async Task QuandJeRetrogradeUnUtilisateurQuiNeParticipePasACetEvenementViaLapi() =>
        await Retrograder(eventContext.EventId!.Value, Guid.NewGuid(), sessionContext.Cookie);

    private async Task Retrograder(Guid eventId, Guid userId, string? cookieHeader)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/events/{eventId}/participants/{userId}/demote");
        if (cookieHeader is not null)
        {
            httpRequest.Headers.Add("Cookie", cookieHeader);
        }

        _response = await client.SendAsync(httpRequest);
    }

    [Then(@"la réponse de rétrogradation a le statut (\d+)")]
    public void AlorsLaReponseDeRetrogradationALeStatut(int expectedStatusCode) =>
        Assert.Equal(expectedStatusCode, (int)_response!.StatusCode);
}
