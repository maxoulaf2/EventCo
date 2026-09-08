using EventCo.Api.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace EventCo.Api.Tests.Events.PromoteToOrganizer;

[Binding]
public sealed class PromoteToOrganizerSteps(SessionContext sessionContext, EventContext eventContext, InvitedParticipantContext invitedParticipantContext)
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };

    private HttpResponseMessage? _response;

    [When(@"je promeus ce participant en co-organisateur via l'API")]
    public async Task QuandJePromeusCeParticipantEnCoOrganisateurViaLapi() =>
        await Promouvoir(eventContext.EventId!.Value, invitedParticipantContext.UserId!.Value, sessionContext.Cookie);

    [When(@"je promeus un participant sur un événement inexistant via l'API")]
    public async Task QuandJePromeusUnParticipantSurUnEvenementInexistantViaLapi() =>
        await Promouvoir(Guid.NewGuid(), Guid.NewGuid(), sessionContext.Cookie);

    [When(@"je promeus un participant sur un événement inexistant via l'API sans cookie de session")]
    public async Task QuandJePromeusUnParticipantSurUnEvenementInexistantViaLapiSansCookieDeSession() =>
        await Promouvoir(Guid.NewGuid(), Guid.NewGuid(), null);

    private async Task Promouvoir(Guid eventId, Guid userId, string? cookieHeader)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/events/{eventId}/participants/{userId}/promote");
        if (cookieHeader is not null)
        {
            httpRequest.Headers.Add("Cookie", cookieHeader);
        }

        _response = await client.SendAsync(httpRequest);
    }

    [Then(@"la réponse de promotion a le statut (\d+)")]
    public void AlorsLaReponseDePromotionALeStatut(int expectedStatusCode) =>
        Assert.Equal(expectedStatusCode, (int)_response!.StatusCode);
}
