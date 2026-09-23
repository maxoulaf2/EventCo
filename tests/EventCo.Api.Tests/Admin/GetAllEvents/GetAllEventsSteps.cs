using System.Net.Http.Json;
using EventCo.Api.Contracts.Admin;
using EventCo.Api.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace EventCo.Api.Tests.Admin.GetAllEvents;

[Binding]
public sealed class GetAllEventsSteps(SessionContext sessionContext, EventContext eventContext)
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };

    private HttpResponseMessage? _response;
    private List<AdminEventSummaryResponse>? _allEvents;

    [When(@"je consulte la liste de tous les événements via l'API")]
    public async Task QuandJeConsulteLaListeDeTousLesEvenementsViaLapi() =>
        await AppelerAvecCookie(sessionContext.Cookie);

    [When(@"je consulte la liste de tous les événements via l'API sans cookie de session")]
    public async Task QuandJeConsulteLaListeDeTousLesEvenementsViaLapiSansCookieDeSession() =>
        await AppelerAvecCookie(null);

    private async Task AppelerAvecCookie(string? cookieHeader)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/events");
        if (cookieHeader is not null)
        {
            request.Headers.Add("Cookie", cookieHeader);
        }

        _response = await client.SendAsync(request);
        if (_response.IsSuccessStatusCode)
        {
            _allEvents = await _response.Content.ReadFromJsonAsync<List<AdminEventSummaryResponse>>();
        }
    }

    [Then(@"la réponse de liste de tous les événements a le statut (\d+)")]
    public void AlorsLaReponseDeListeDeTousLesEvenementsALeStatut(int expectedStatusCode) =>
        Assert.Equal(expectedStatusCode, (int)_response!.StatusCode);

    // Base partagée entre scénarios (Testcontainers démarré une fois par run) : on cible l'événement
    // créé par ce scénario plutôt que de compter le nombre total d'événements retournés.
    [Then(@"la liste de tous les événements retournée contient cet événement avec (\d+) participants?")]
    public void AlorsLaListeDeTousLesEvenementsRetourneeContientCetEvenementAvecParticipants(int participantCount)
    {
        var summary = Assert.Single(_allEvents!, e => e.Id == eventContext.EventId);
        Assert.Equal(participantCount, summary.ParticipantCount);
    }
}
