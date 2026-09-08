using System.Net.Http.Json;
using EventCo.Api.Contracts.Events;
using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;

namespace EventCo.Api.Tests.Support;

[Binding]
public sealed class ExistingTaskSteps(SessionContext sessionContext, EventContext eventContext, TaskContext taskContext)
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };

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
        taskContext.TaskId = task!.Id;
    }
}
