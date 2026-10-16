using EventCo.Api.Tests.Support;
using EventCo.Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;
using Reqnroll;

namespace EventCo.Api.Tests.Events.ItemRealtimeNotifications;

[Binding]
public sealed class ItemRealtimeNotificationsSteps(RealtimeHubConnectionContext hubConnectionContext)
{
    private static readonly TimeSpan ReceiveTimeout = TimeSpan.FromSeconds(5);

    private readonly TaskCompletionSource<ItemRealtimeDto> _itemCreated = new();
    private readonly TaskCompletionSource<ItemRealtimeDto> _itemAssigned = new();
    private readonly TaskCompletionSource<ItemRealtimeDto> _itemCategoryCorrected = new();
    private readonly TaskCompletionSource<ItemDeletedRealtimeDto> _itemDeleted = new();

    [Given(@"j'écoute les notifications temps réel d'articles")]
    public void EtantDonneJecouteLesNotificationsTempsReelDarticles()
    {
        var connection = hubConnectionContext.Connection!;

        connection.On<ItemRealtimeDto>("ItemCreated", item => _itemCreated.TrySetResult(item));
        connection.On<ItemRealtimeDto>("ItemAssigned", item => _itemAssigned.TrySetResult(item));
        connection.On<ItemRealtimeDto>("ItemCategoryCorrected", item => _itemCategoryCorrected.TrySetResult(item));
        connection.On<ItemDeletedRealtimeDto>("ItemDeleted", item => _itemDeleted.TrySetResult(item));
    }

    [Then(@"une notification temps réel de création d'article est reçue pour cet article")]
    public async Task AlorsUneNotificationTempsReelDeCreationDarticleEstRecuePourCetArticle() =>
        await AttendreReception(_itemCreated.Task);

    [Then(@"une notification temps réel d'assignation d'article est reçue pour cet article")]
    public async Task AlorsUneNotificationTempsReelDassignationDarticleEstRecuePourCetArticle()
    {
        var item = await AttendreReception(_itemAssigned.Task);
        Assert.NotNull(item.AssignedToUserId);
    }

    [Then(@"une notification temps réel de correction de catégorie d'article est reçue pour cet article avec la catégorie ""(.*)""")]
    public async Task AlorsUneNotificationTempsReelDeCorrectionDeCategorieDarticleEstRecuePourCetArticle(string category)
    {
        var item = await AttendreReception(_itemCategoryCorrected.Task);
        Assert.Equal(category, item.Category);
    }

    [Then(@"une notification temps réel de suppression d'article est reçue pour cet article")]
    public async Task AlorsUneNotificationTempsReelDeSuppressionDarticleEstRecuePourCetArticle() =>
        await AttendreReception(_itemDeleted.Task);

    private static async Task<T> AttendreReception<T>(Task<T> reception)
    {
        var completed = await Task.WhenAny(reception, Task.Delay(ReceiveTimeout));
        Assert.Same(reception, completed);
        return await reception;
    }
}
