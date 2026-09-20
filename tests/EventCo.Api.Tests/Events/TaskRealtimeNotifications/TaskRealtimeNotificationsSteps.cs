using EventCo.Api.Tests.Support;
using EventCo.Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;
using Reqnroll;

namespace EventCo.Api.Tests.Events.TaskRealtimeNotifications;

[Binding]
public sealed class TaskRealtimeNotificationsSteps(RealtimeHubConnectionContext hubConnectionContext)
{
    private static readonly TimeSpan ReceiveTimeout = TimeSpan.FromSeconds(5);

    private readonly TaskCompletionSource<TaskRealtimeDto> _taskCreated = new();
    private readonly TaskCompletionSource<TaskRealtimeDto> _taskAssigned = new();
    private readonly TaskCompletionSource<TaskDeletedRealtimeDto> _taskDeleted = new();

    [Given(@"j'écoute les notifications temps réel de tâches")]
    public void EtantDonneJecouteLesNotificationsTempsReelDeTaches()
    {
        var connection = hubConnectionContext.Connection!;

        connection.On<TaskRealtimeDto>("TaskCreated", task => _taskCreated.TrySetResult(task));
        connection.On<TaskRealtimeDto>("TaskAssigned", task => _taskAssigned.TrySetResult(task));
        connection.On<TaskDeletedRealtimeDto>("TaskDeleted", task => _taskDeleted.TrySetResult(task));
    }

    [Then(@"une notification temps réel de création de tâche est reçue pour cette tâche")]
    public async Task AlorsUneNotificationTempsReelDeCreationDeTacheEstRecuePourCetteTache() =>
        await AttendreReception(_taskCreated.Task);

    [Then(@"une notification temps réel d'assignation de tâche est reçue pour cette tâche")]
    public async Task AlorsUneNotificationTempsReelDassignationDeTacheEstRecuePourCetteTache()
    {
        var task = await AttendreReception(_taskAssigned.Task);
        Assert.NotNull(task.AssignedToUserId);
    }

    [Then(@"une notification temps réel de suppression de tâche est reçue pour cette tâche")]
    public async Task AlorsUneNotificationTempsReelDeSuppressionDeTacheEstRecuePourCetteTache() =>
        await AttendreReception(_taskDeleted.Task);

    private static async Task<T> AttendreReception<T>(Task<T> task)
    {
        var completed = await Task.WhenAny(task, Task.Delay(ReceiveTimeout));
        Assert.Same(task, completed);
        return await task;
    }
}
