using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Events.AssignItem;
using EventCo.Application.Events.CreateEvent;
using EventCo.Application.Events.CreateItem;
using EventCo.Application.Events.InviteParticipant;
using EventCo.Application.Events.UnassignItem;
using EventCo.Application.Tests.Support;
using EventCo.Application.Tests.TestDoubles;
using EventCo.Domain.Events.Exceptions;
using EventCo.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;

namespace EventCo.Application.Tests.Events.UnassignItem;

[Binding]
public sealed class UnassignItemSteps
{
    private readonly IServiceProvider _serviceProvider;
    private readonly CurrentUserContext _currentUserContext;
    private readonly RecordingItemRealtimeNotifier _itemRealtimeNotifier = new();
    private readonly DateTime _now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private Guid? _existingEventId;
    private Guid? _existingItemId;
    private Guid? _assignedParticipantUserId;
    private Exception? _thrownException;

    public UnassignItemSteps(CurrentUserContext currentUserContext)
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

    [Given(@"un événement ""(.*)"" avec un article ""(.*)"" déjà assigné à un participant, prévu le ""(.*)"" au lieu ""(.*)""")]
    public async Task EtantDonneUnEvenementAvecUnArticleDejaAssigneAUnParticipantPrevuLeAuLieu(string title, string itemTitle, string eventDate, string location)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        var createResult = await dispatcher.Send(
            new CreateEventCommand(title, null, DateTime.Parse(eventDate), location),
            CancellationToken.None);
        _existingEventId = createResult.EventId;

        var itemResult = await dispatcher.Send(
            new CreateItemCommand(_existingEventId.Value, itemTitle, "1"),
            CancellationToken.None);
        _existingItemId = itemResult.ItemId;

        var inviteResult = await dispatcher.Send(
            new InviteParticipantCommand(_existingEventId.Value, "assignee@example.com"),
            CancellationToken.None);
        _assignedParticipantUserId = inviteResult.UserId;

        await dispatcher.Send(
            new AssignItemCommand(_existingEventId.Value, _existingItemId.Value, _assignedParticipantUserId.Value),
            CancellationToken.None);
    }

    [Given(@"je deviens ce participant assigné")]
    public void EtantDonneJeDeviensCeParticipantAssigne() => _currentUserContext.UserId = _assignedParticipantUserId!.Value;

    [Given(@"un second participant a rejoint cet événement et devient l'utilisateur courant")]
    public async Task EtantDonneUnSecondParticipantARejointCetEvenementEtDevientLutilisateurCourant()
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        var inviteResult = await dispatcher.Send(
            new InviteParticipantCommand(_existingEventId!.Value, "autre@example.com"),
            CancellationToken.None);

        _currentUserContext.UserId = inviteResult.UserId;
    }

    [When(@"je désassigne cet article")]
    public async Task QuandJeDesassigneCetArticle() =>
        await Desassigner(_existingEventId!.Value, _existingItemId!.Value);

    [When(@"je désassigne un article sur un événement inexistant")]
    public async Task QuandJeDesassigneUnArticleSurUnEvenementInexistant() =>
        await Desassigner(Guid.NewGuid(), Guid.NewGuid());

    [When(@"je désassigne un article inexistant sur cet événement")]
    public async Task QuandJeDesassigneUnArticleInexistantSurCetEvenement() =>
        await Desassigner(_existingEventId!.Value, Guid.NewGuid());

    private async Task Desassigner(Guid eventId, Guid itemId)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        _thrownException = null;

        try
        {
            await dispatcher.Send(new UnassignItemCommand(eventId, itemId), CancellationToken.None);
        }
        catch (Exception exception)
        {
            _thrownException = exception;
        }
    }

    [Then(@"la désassignation réussit")]
    public void AlorsLaDesassignationReussit() => Assert.Null(_thrownException);

    [Then(@"la désassignation échoue avec une erreur de désassignation réservée à l'assigné")]
    public void AlorsLaDesassignationEchoueAvecUneErreurDeDesassignationReserveeALassigne() =>
        Assert.IsType<ParticipantCannotUnassignOthersItemException>(_thrownException);

    [Then(@"la désassignation échoue avec une erreur d'autorisation")]
    public void AlorsLaDesassignationEchoueAvecUneErreurDautorisation() =>
        Assert.IsType<UserNotEventParticipantException>(_thrownException);

    [Then(@"la désassignation échoue avec une erreur d'événement introuvable")]
    public void AlorsLaDesassignationEchoueAvecUneErreurDevenementIntrouvable() =>
        Assert.IsType<EventNotFoundException>(_thrownException);

    [Then(@"la désassignation échoue avec une erreur d'article introuvable")]
    public void AlorsLaDesassignationEchoueAvecUneErreurDarticleIntrouvable() =>
        Assert.IsType<EventItemNotFoundException>(_thrownException);

    [Then(@"l'article n'est plus assigné")]
    public async Task AlorsLarticleNestPlusAssigne()
    {
        var eventRepository = _serviceProvider.GetRequiredService<IEventRepository>();
        var @event = await eventRepository.GetByIdAsync(_existingEventId!.Value, CancellationToken.None);
        var item = @event!.Items.Single(t => t.Id == _existingItemId!.Value);

        Assert.Null(item.AssignedToUserId);
    }

    [Then(@"une notification temps réel de désassignation d'article est diffusée")]
    public void AlorsUneNotificationTempsReelDeDesassignationDarticleEstDiffusee()
    {
        var notification = Assert.Single(_itemRealtimeNotifier.ItemUnassignedNotifications);
        Assert.Equal(_existingItemId!.Value, notification.ItemId);
        Assert.Null(notification.AssignedToUserId);
    }
}
