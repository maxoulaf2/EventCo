using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Events.CreateEvent;
using EventCo.Application.Events.CreateItem;
using EventCo.Application.Events.DeleteItem;
using EventCo.Application.Events.InviteParticipant;
using EventCo.Application.Tests.Support;
using EventCo.Application.Tests.TestDoubles;
using EventCo.Domain.Events.Exceptions;
using EventCo.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;

namespace EventCo.Application.Tests.Events.DeleteItem;

[Binding]
public sealed class DeleteItemSteps
{
    private readonly IServiceProvider _serviceProvider;
    private readonly CurrentUserContext _currentUserContext;
    private readonly RecordingItemRealtimeNotifier _itemRealtimeNotifier = new();
    private readonly DateTime _now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private Guid? _existingEventId;
    private Guid? _existingItemId;
    private Guid? _eventCreatorUserId;
    private Guid? _itemCreatorUserId;
    private Guid? _otherInvitedUserId;
    private Exception? _thrownException;

    public DeleteItemSteps(CurrentUserContext currentUserContext)
    {
        _currentUserContext = currentUserContext;

        var builder = new ApplicationTestHostBuilder();

        builder.Services.AddScoped<IEventRepository, EventRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddSingleton<IDateTimeProvider>(new FixedDateTimeProvider(_now));
        builder.Services.AddScoped<ICurrentUserService>(_ => new CurrentUserContextService(currentUserContext));
        builder.Services.AddSingleton<IItemRealtimeNotifier>(_itemRealtimeNotifier);

        _serviceProvider = builder.Build();
    }

    [Given(@"un événement ""(.*)"" avec un article créé par un participant invité ""(.*)"", prévu le ""(.*)"" au lieu ""(.*)""")]
    public async Task EtantDonneUnEvenementAvecUnArticleCreeParUnParticipantInvitePrevuLeAuLieu(string title, string itemTitle, string eventDate, string location)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        _eventCreatorUserId = _currentUserContext.UserId;

        var createResult = await dispatcher.Send(
            new CreateEventCommand(title, null, DateTime.Parse(eventDate), location),
            CancellationToken.None);
        _existingEventId = createResult.EventId;

        var itemCreatorInviteResult = await dispatcher.Send(
            new InviteParticipantCommand(_existingEventId.Value, "createur-article@example.com"),
            CancellationToken.None);
        _itemCreatorUserId = itemCreatorInviteResult.UserId;

        var otherInviteResult = await dispatcher.Send(
            new InviteParticipantCommand(_existingEventId.Value, "autre-participant@example.com"),
            CancellationToken.None);
        _otherInvitedUserId = otherInviteResult.UserId;

        _currentUserContext.UserId = _itemCreatorUserId.Value;

        var itemResult = await dispatcher.Send(
            new CreateItemCommand(_existingEventId.Value, itemTitle, "1"),
            CancellationToken.None);
        _existingItemId = itemResult.ItemId;
    }

    [Given(@"je redeviens le créateur de l'événement")]
    public void EtantDonneJeRedeviensLeCreateurDeLevenement() =>
        _currentUserContext.UserId = _eventCreatorUserId!.Value;

    [Given(@"un autre participant invité devient l'utilisateur courant")]
    public void EtantDonneUnAutreParticipantInviteDevientLutilisateurCourant() =>
        _currentUserContext.UserId = _otherInvitedUserId!.Value;

    [When(@"je supprime cet article")]
    public async Task QuandJeSupprimeCetArticle() =>
        await Supprimer(_existingEventId!.Value, _existingItemId!.Value);

    [When(@"je supprime un article sur un événement inexistant")]
    public async Task QuandJeSupprimeUnArticleSurUnEvenementInexistant() =>
        await Supprimer(Guid.NewGuid(), Guid.NewGuid());

    [When(@"je supprime un article inexistant sur cet événement")]
    public async Task QuandJeSupprimeUnArticleInexistantSurCetEvenement() =>
        await Supprimer(_existingEventId!.Value, Guid.NewGuid());

    private async Task Supprimer(Guid eventId, Guid itemId)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        _thrownException = null;

        try
        {
            await dispatcher.Send(new DeleteItemCommand(eventId, itemId), CancellationToken.None);
        }
        catch (Exception exception)
        {
            _thrownException = exception;
        }
    }

    [Then(@"la suppression de l'article réussit")]
    public void AlorsLaSuppressionDeLarticleReussit() => Assert.Null(_thrownException);

    [Then(@"la suppression de l'article échoue avec une erreur de suppression réservée au créateur de l'article")]
    public void AlorsLaSuppressionDeLarticleEchoueAvecUneErreurDeSuppressionReserveeAuCreateurDeLarticle() =>
        Assert.IsType<ParticipantCannotDeleteOthersItemException>(_thrownException);

    [Then(@"la suppression de l'article échoue avec une erreur d'autorisation")]
    public void AlorsLaSuppressionDeLarticleEchoueAvecUneErreurDautorisation() =>
        Assert.IsType<UserNotEventParticipantException>(_thrownException);

    [Then(@"la suppression de l'article échoue avec une erreur d'événement introuvable")]
    public void AlorsLaSuppressionDeLarticleEchoueAvecUneErreurDevenementIntrouvable() =>
        Assert.IsType<EventNotFoundException>(_thrownException);

    [Then(@"la suppression de l'article échoue avec une erreur d'article introuvable")]
    public void AlorsLaSuppressionDeLarticleEchoueAvecUneErreurDarticleIntrouvable() =>
        Assert.IsType<EventItemNotFoundException>(_thrownException);

    [Then(@"l'article n'existe plus")]
    public async Task AlorsLarticleNexistePlus()
    {
        var eventRepository = _serviceProvider.GetRequiredService<IEventRepository>();
        var @event = await eventRepository.GetByIdAsync(_existingEventId!.Value, CancellationToken.None);

        Assert.DoesNotContain(@event!.Items, t => t.Id == _existingItemId!.Value);
    }

    [Then(@"une notification temps réel de suppression d'article est diffusée")]
    public void AlorsUneNotificationTempsReelDeSuppressionDarticleEstDiffusee()
    {
        var notification = Assert.Single(_itemRealtimeNotifier.ItemDeletedNotifications);
        Assert.Equal(_existingItemId!.Value, notification.ItemId);
        Assert.Equal(_existingEventId!.Value, notification.EventId);
    }
}
