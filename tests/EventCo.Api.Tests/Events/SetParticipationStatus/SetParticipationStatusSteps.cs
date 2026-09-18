using System.Net.Http.Json;
using EventCo.Api.Contracts.Events;
using EventCo.Api.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace EventCo.Api.Tests.Events.SetParticipationStatus;

[Binding]
public sealed class SetParticipationStatusSteps(SessionContext sessionContext, EventContext eventContext)
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };

    private HttpResponseMessage? _response;

    [When(@"j'indique le statut de participation ""(.*)"" via l'API")]
    public async Task QuandJindiqueLeStatutDeParticipationViaLapi(string status) =>
        await Indiquer(eventContext.EventId!.Value, status, sessionContext.Cookie);

    [When(@"j'indique le statut de participation ""(.*)"" sur un événement inexistant via l'API")]
    public async Task QuandJindiqueLeStatutDeParticipationSurUnEvenementInexistantViaLapi(string status) =>
        await Indiquer(Guid.NewGuid(), status, sessionContext.Cookie);

    [When(@"j'indique le statut de participation ""(.*)"" sur un événement inexistant via l'API sans cookie de session")]
    public async Task QuandJindiqueLeStatutDeParticipationSurUnEvenementInexistantViaLapiSansCookieDeSession(string status) =>
        await Indiquer(Guid.NewGuid(), status, null);

    private async Task Indiquer(Guid eventId, string status, string? cookieHeader)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/events/{eventId}/participation-status")
        {
            Content = JsonContent.Create(new SetParticipationStatusRequest(status)),
        };
        if (cookieHeader is not null)
        {
            httpRequest.Headers.Add("Cookie", cookieHeader);
        }

        _response = await client.SendAsync(httpRequest);
    }

    [Then(@"la réponse de changement de statut a le statut (\d+)")]
    public void AlorsLaReponseDeChangementDeStatutALeStatut(int expectedStatusCode) =>
        Assert.Equal(expectedStatusCode, (int)_response!.StatusCode);
}
