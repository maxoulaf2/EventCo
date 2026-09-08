using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace EventCo.Api.Tests.Support;

[Binding]
public sealed class PromotedParticipantSteps(SessionContext sessionContext, EventContext eventContext, InvitedParticipantContext invitedParticipantContext)
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };

    [Given(@"ce participant est déjà promu co-organisateur via l'API")]
    public async Task EtantDonneCeParticipantEstDejaPromuCoOrganisateurViaLapi()
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/events/{eventContext.EventId}/participants/{invitedParticipantContext.UserId}/promote");
        httpRequest.Headers.Add("Cookie", sessionContext.Cookie!);

        await client.SendAsync(httpRequest);
    }
}
