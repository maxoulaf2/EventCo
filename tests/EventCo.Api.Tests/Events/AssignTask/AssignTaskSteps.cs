using System.Net.Http.Json;
using EventCo.Api.Contracts.Events;
using EventCo.Api.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace EventCo.Api.Tests.Events.AssignTask;

[Binding]
public sealed class AssignTaskSteps(SessionContext sessionContext, EventContext eventContext, InvitedParticipantContext invitedParticipantContext)
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };

    private Guid? _taskId;
    private Guid? _secondParticipantUserId;
    private HttpResponseMessage? _response;

    [Given(@"une tâche ""(.*)"" ajoutée à cet événement via l'API")]
    public async Task EtantDonneUneTacheAjouteeACetEvenementViaLapi(string title)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/events/{eventContext.EventId}/tasks")
        {
            Content = JsonContent.Create(new CreateTaskRequest(title, "Courses", "1")),
        };
        httpRequest.Headers.Add("Cookie", sessionContext.Cookie!);

        var response = await client.SendAsync(httpRequest);
        var task = await response.Content.ReadFromJsonAsync<EventTaskResponse>();
        _taskId = task!.Id;
    }

    [Given(@"""(.*)"" également invité à cet événement via l'API")]
    public async Task EtantDonneEgalementInviteACetEvenementViaLapi(string email)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/events/{eventContext.EventId}/participants")
        {
            Content = JsonContent.Create(new InviteParticipantRequest(email)),
        };
        httpRequest.Headers.Add("Cookie", sessionContext.Cookie!);

        var response = await client.SendAsync(httpRequest);
        var participant = await response.Content.ReadFromJsonAsync<EventParticipantResponse>();
        _secondParticipantUserId = participant!.UserId;
    }

    [When(@"j'assigne cette tâche à ce participant via l'API")]
    public async Task QuandJassigneCetteTacheACeParticipantViaLapi() =>
        await Assigner(eventContext.EventId!.Value, _taskId!.Value, invitedParticipantContext.UserId!.Value, sessionContext.Cookie);

    [When(@"j'assigne cette tâche à l'autre participant via l'API")]
    public async Task QuandJassigneCetteTacheALautreParticipantViaLapi() =>
        await Assigner(eventContext.EventId!.Value, _taskId!.Value, _secondParticipantUserId!.Value, sessionContext.Cookie);

    [When(@"j'assigne cette tâche à un utilisateur qui n'est pas participant via l'API")]
    public async Task QuandJassigneCetteTacheAUnUtilisateurQuiNestPasParticipantViaLapi() =>
        await Assigner(eventContext.EventId!.Value, _taskId!.Value, Guid.NewGuid(), sessionContext.Cookie);

    [When(@"j'assigne une tâche à un événement inexistant via l'API")]
    public async Task QuandJassigneUneTacheAUnEvenementInexistantViaLapi() =>
        await Assigner(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), sessionContext.Cookie);

    [When(@"j'assigne une tâche à un événement inexistant via l'API sans cookie de session")]
    public async Task QuandJassigneUneTacheAUnEvenementInexistantViaLapiSansCookieDeSession() =>
        await Assigner(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null);

    private async Task Assigner(Guid eventId, Guid taskId, Guid userId, string? cookieHeader)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/events/{eventId}/tasks/{taskId}/assign/{userId}");
        if (cookieHeader is not null)
        {
            httpRequest.Headers.Add("Cookie", cookieHeader);
        }

        _response = await client.SendAsync(httpRequest);
    }

    [Then(@"la réponse d'assignation a le statut (\d+)")]
    public void AlorsLaReponseDassignationALeStatut(int expectedStatusCode) =>
        Assert.Equal(expectedStatusCode, (int)_response!.StatusCode);
}
