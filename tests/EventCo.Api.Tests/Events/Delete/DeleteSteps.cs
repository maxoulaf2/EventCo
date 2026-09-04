using EventCo.Api.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace EventCo.Api.Tests.Events.Delete;

[Binding]
public sealed class DeleteSteps(SessionContext sessionContext, EventContext eventContext)
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };

    private HttpResponseMessage? _response;

    [When(@"je supprime cet événement via l'API")]
    public async Task QuandJeSupprimeCetEvenementViaLapi() =>
        await AppelerAvecCookie(eventContext.EventId!.Value, sessionContext.Cookie);

    [When(@"je supprime un événement inexistant via l'API")]
    public async Task QuandJeSupprimeUnEvenementInexistantViaLapi() =>
        await AppelerAvecCookie(Guid.NewGuid(), sessionContext.Cookie);

    [When(@"je supprime un événement inexistant via l'API sans cookie de session")]
    public async Task QuandJeSupprimeUnEvenementInexistantViaLapiSansCookieDeSession() =>
        await AppelerAvecCookie(Guid.NewGuid(), null);

    private async Task AppelerAvecCookie(Guid eventId, string? cookieHeader)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/events/{eventId}");
        if (cookieHeader is not null)
        {
            httpRequest.Headers.Add("Cookie", cookieHeader);
        }

        _response = await client.SendAsync(httpRequest);
    }

    [Then(@"la réponse de suppression d'événement a le statut (\d+)")]
    public void AlorsLaReponseDeSuppressionDevenementALeStatut(int expectedStatusCode) =>
        Assert.Equal(expectedStatusCode, (int)_response!.StatusCode);
}
