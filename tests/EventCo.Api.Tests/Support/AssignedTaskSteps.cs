using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace EventCo.Api.Tests.Support;

[Binding]
public sealed class AssignedTaskSteps(SessionContext sessionContext, EventContext eventContext, TaskContext taskContext, InvitedParticipantContext invitedParticipantContext)
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };

    [Given(@"cette tâche assignée à ce participant via l'API")]
    public async Task EtantDonneCetteTacheAssigneeACeParticipantViaLapi()
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/events/{eventContext.EventId}/tasks/{taskContext.TaskId}/assign/{invitedParticipantContext.UserId}");
        httpRequest.Headers.Add("Cookie", sessionContext.Cookie!);

        await client.SendAsync(httpRequest);
    }
}
