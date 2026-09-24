using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace EventCo.Api.Tests.Support;

[Binding]
public sealed class AssignedItemSteps(SessionContext sessionContext, EventContext eventContext, ItemContext itemContext, InvitedParticipantContext invitedParticipantContext)
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };

    [Given(@"cet article assigné à ce participant via l'API")]
    public async Task EtantDonneCetArticleAssigneACeParticipantViaLapi()
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/events/{eventContext.EventId}/items/{itemContext.ItemId}/assign/{invitedParticipantContext.UserId}");
        httpRequest.Headers.Add("Cookie", sessionContext.Cookie!);

        await client.SendAsync(httpRequest);
    }
}
