using System.Net.Http.Json;
using System.Text.Json;
using EventCo.Api.Contracts.Events;
using EventCo.Api.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Reqnroll;

namespace EventCo.Api.Tests.Events.DatePoll;

// Les options sont désignées par un nombre de jours à partir d'aujourd'hui : l'API tourne avec la
// vraie horloge et refuse une option de date passée.
[Binding]
public sealed class DatePollSteps(
    SessionContext sessionContext,
    EventContext eventContext,
    InvitedParticipantContext invitedParticipantContext,
    RealtimeHubConnectionContext hubConnectionContext)
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };
    private static readonly TimeSpan ReceiveTimeout = TimeSpan.FromSeconds(5);

    private readonly TaskCompletionSource<JsonElement> _datePollChanged = new();

    private HttpResponseMessage? _response;
    private List<EventSummaryResponse>? _myEvents;

    private static DateTime InDays(int days) => DateTime.UtcNow.Date.AddDays(days);

    private async Task<HttpResponseMessage> Send(HttpMethod method, string url, object? body = null, bool withCookie = true)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);

        using var httpRequest = new HttpRequestMessage(method, url);
        if (body is not null)
            httpRequest.Content = JsonContent.Create(body);
        if (withCookie)
            httpRequest.Headers.Add("Cookie", sessionContext.Cookie!);

        return await client.SendAsync(httpRequest);
    }

    private async Task<EventDetailResponse> GetDetail()
    {
        var response = await Send(HttpMethod.Get, $"/api/events/{eventContext.EventId}");
        return (await response.Content.ReadFromJsonAsync<EventDetailResponse>())!;
    }

    private async Task<string> OptionUrl(int days)
    {
        var option = (await GetDetail()).DateOptions.Single(o => o.Date == InDays(days));
        return $"/api/events/{eventContext.EventId}/date-options/{option.Id}";
    }

    [Given(@"un événement avec un sondage proposant des dates dans ""(.*)"" jours via l'API")]
    [When(@"je crée un événement avec un sondage proposant des dates dans ""(.*)"" jours via l'API")]
    public async Task JeCreeUnEvenementAvecUnSondage(string days)
    {
        var dateOptions = days.Split(',').Select(d => InDays(int.Parse(d))).ToList();
        _response = await Send(HttpMethod.Post, "/api/events", new CreateEventRequest("Week-end", null, null, "Lyon", dateOptions));

        if (_response.IsSuccessStatusCode)
        {
            var createdEvent = await _response.Content.ReadFromJsonAsync<EventResponse>();
            eventContext.EventId = createdEvent!.Id;
            eventContext.CreatedByUserId = createdEvent.CreatedByUserId;
        }
    }

    [Given(@"je vote pour la date dans (\d+) jours via l'API")]
    [When(@"je vote pour la date dans (\d+) jours via l'API")]
    public async Task JeVotePourLaDate(int days) =>
        _response = await Send(HttpMethod.Put, await OptionUrl(days) + "/vote");

    [When(@"je retire mon vote pour la date dans (\d+) jours via l'API")]
    public async Task JeRetireMonVote(int days) =>
        _response = await Send(HttpMethod.Delete, await OptionUrl(days) + "/vote");

    [Given(@"j'ajoute une option de date dans (\d+) jours via l'API")]
    public async Task JajouteUneOptionDeDate(int days) =>
        _response = await Send(HttpMethod.Post, $"/api/events/{eventContext.EventId}/date-options", new AddDateOptionRequest(InDays(days)));

    [When(@"je supprime l'option de date dans (\d+) jours via l'API")]
    public async Task JeSupprimeLoptionDeDate(int days) =>
        _response = await Send(HttpMethod.Delete, await OptionUrl(days));

    [When(@"je retiens la date dans (\d+) jours via l'API")]
    public async Task JeRetiensLaDate(int days) =>
        _response = await Send(HttpMethod.Post, await OptionUrl(days) + "/choose");

    [When(@"je vote pour une option de date inexistante via l'API")]
    public async Task JeVotePourUneOptionInexistante() =>
        _response = await Send(HttpMethod.Put, $"/api/events/{eventContext.EventId}/date-options/{Guid.NewGuid()}/vote");

    [When(@"je vote pour une option de date sans cookie de session")]
    public async Task JeVoteSansCookieDeSession() =>
        _response = await Send(HttpMethod.Put, $"/api/events/{Guid.NewGuid()}/date-options/{Guid.NewGuid()}/vote", withCookie: false);

    [When(@"je supprime cet événement en sondage via l'API")]
    public async Task JeSupprimeCetEvenementEnSondage() =>
        _response = await Send(HttpMethod.Delete, $"/api/events/{eventContext.EventId}");

    [When(@"je consulte mes événements après le sondage via l'API")]
    public async Task JeConsulteMesEvenements()
    {
        var response = await Send(HttpMethod.Get, "/api/events");
        _myEvents = await response.Content.ReadFromJsonAsync<List<EventSummaryResponse>>();
    }

    [Given(@"j'écoute les changements du sondage en temps réel")]
    public void JecouteLesChangementsDuSondage() =>
        hubConnectionContext.Connection!.On<JsonElement>("DatePollChanged", payload => _datePollChanged.TrySetResult(payload));

    [Then(@"la réponse du sondage a le statut (\d+)")]
    public void LaReponseDuSondageALeStatut(int expectedStatusCode) =>
        Assert.Equal(expectedStatusCode, (int)_response!.StatusCode);

    [Then(@"le détail de l'événement n'a pas de date et propose (\d+) options")]
    public async Task LeDetailNaPasDeDateEtPropose(int count)
    {
        var detail = await GetDetail();
        Assert.Null(detail.EventDate);
        Assert.Equal(count, detail.DateOptions.Count);
    }

    [Given(@"le détail de l'événement montre (\d+) votes? pour la date dans (\d+) jours")]
    [Then(@"le détail de l'événement montre (\d+) votes? pour la date dans (\d+) jours")]
    public async Task LeDetailMontreDesVotes(int count, int days) =>
        Assert.Equal(count, (await GetDetail()).DateOptions.Single(o => o.Date == InDays(days)).VoterUserIds.Count);

    [Then(@"le détail de l'événement a pour date celle dans (\d+) jours")]
    public async Task LeDetailAPourDate(int days) => Assert.Equal(InDays(days), (await GetDetail()).EventDate);

    [Then(@"le participant invité est indiqué présent dans le détail de l'événement")]
    public async Task LeParticipantInviteEstIndiquePresent() =>
        Assert.Equal("Attending", (await GetDetail()).Participants.Single(p => p.UserId == invitedParticipantContext.UserId).ParticipationStatus);

    [Then(@"le premier événement listé n'a pas de date")]
    public void LePremierEvenementListeNaPasDeDate()
    {
        Assert.Equal(2, _myEvents!.Count);
        Assert.Null(_myEvents[0].EventDate);
    }

    [Then(@"un changement du sondage est reçu en temps réel")]
    public async Task UnChangementDuSondageEstRecu()
    {
        var completed = await Task.WhenAny(_datePollChanged.Task, Task.Delay(ReceiveTimeout));
        Assert.Same(_datePollChanged.Task, completed);
        Assert.Equal(eventContext.EventId, (await _datePollChanged.Task).GetProperty("eventId").GetGuid());
    }
}
