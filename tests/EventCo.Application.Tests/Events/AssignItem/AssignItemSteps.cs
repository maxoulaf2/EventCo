using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Events.AssignItem;
using EventCo.Application.Events.CreateEvent;
using EventCo.Application.Events.CreateItem;
using EventCo.Application.Events.InviteParticipant;
using EventCo.Application.Tests.Support;
using EventCo.Application.Tests.TestDoubles;
using EventCo.Domain.Events.Exceptions;
using EventCo.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;

namespace EventCo.Application.Tests.Events.AssignItem;

[Binding]
public sealed class AssignItemSteps
{
    private readonly IServiceProvider _serviceProvider;
    private readonly CurrentUserContext _currentUserContext;
    private readonly RecordingItemRealtimeNotifier _itemRealtimeNotifier = new();
    private readonly DateTime _now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private Guid? _existingEventId;
    private Guid? _existingItemId;
    private Guid? _firstParticipantUserId;
    private Guid? _secondParticipantUserId;
    private Exception? _thrownException;

    public AssignItemSteps(CurrentUserContext currentUserContext)
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

    [Given(@"un événement ""(.*)"" avec un article ""(.*)"", prévu le ""(.*)"" au lieu ""(.*)""")]
    public async Task EtantDonneUnEvenementAvecUnArticlePrevuLeAuLieu(string title, string itemTitle, string eventDate, string location)
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
    }

    [Given(@"""(.*)"" rejoint cet événement en tant que participant simple")]
    public async Task EtantDonneRejointCetEvenementEnTantQueParticipantSimple(string email)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        var inviteResult = await dispatcher.Send(
            new InviteParticipantCommand(_existingEventId!.Value, email),
            CancellationToken.None);

        if (_firstParticipantUserId is null)
            _firstParticipantUserId = inviteResult.UserId;
        else
            _secondParticipantUserId = inviteResult.UserId;
    }

    [Given(@"ce participant devient l'utilisateur courant")]
    public void EtantDonneCeParticipantDevientLutilisateurCourant() => _currentUserContext.UserId = _firstParticipantUserId!.Value;

    [When(@"j'assigne cet article à moi-même")]
    public async Task QuandJassigneCetArticleAMoiMeme() =>
        await Assigner(_existingEventId!.Value, _existingItemId!.Value, _currentUserContext.UserId);

    [When(@"j'assigne cet article à ce participant")]
    public async Task QuandJassigneCetArticleACeParticipant() =>
        await Assigner(_existingEventId!.Value, _existingItemId!.Value, _firstParticipantUserId!.Value);

    [When(@"j'assigne cet article à l'autre participant")]
    public async Task QuandJassigneCetArticleALautreParticipant() =>
        await Assigner(_existingEventId!.Value, _existingItemId!.Value, _secondParticipantUserId!.Value);

    [When(@"j'assigne cet article à un utilisateur qui n'est pas participant")]
    public async Task QuandJassigneCetArticleAUnUtilisateurQuiNestPasParticipant() =>
        await Assigner(_existingEventId!.Value, _existingItemId!.Value, Guid.NewGuid());

    [When(@"j'assigne un article à un événement inexistant")]
    public async Task QuandJassigneUnArticleAUnEvenementInexistant() =>
        await Assigner(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

    [When(@"j'assigne un article inexistant à moi-même")]
    public async Task QuandJassigneUnArticleInexistantAMoiMeme() =>
        await Assigner(_existingEventId!.Value, Guid.NewGuid(), _currentUserContext.UserId);

    private async Task Assigner(Guid eventId, Guid itemId, Guid userId)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        _thrownException = null;

        try
        {
            await dispatcher.Send(new AssignItemCommand(eventId, itemId, userId), CancellationToken.None);
        }
        catch (Exception exception)
        {
            _thrownException = exception;
        }
    }

    [Then(@"l'assignation réussit")]
    public void AlorsLassignationReussit() => Assert.Null(_thrownException);

    [Then(@"l'assignation échoue avec une erreur d'auto-assignation")]
    public void AlorsLassignationEchoueAvecUneErreurDautoAssignation() =>
        Assert.IsType<ParticipantCannotAssignItemToOthersException>(_thrownException);

    [Then(@"l'assignation échoue avec une erreur d'assigné invalide")]
    public void AlorsLassignationEchoueAvecUneErreurDassigneInvalide() =>
        Assert.IsType<ItemAssigneeNotParticipantException>(_thrownException);

    [Then(@"l'assignation échoue avec une erreur d'autorisation")]
    public void AlorsLassignationEchoueAvecUneErreurDautorisation() =>
        Assert.IsType<UserNotEventParticipantException>(_thrownException);

    [Then(@"l'assignation échoue avec une erreur d'événement introuvable")]
    public void AlorsLassignationEchoueAvecUneErreurDevenementIntrouvable() =>
        Assert.IsType<EventNotFoundException>(_thrownException);

    [Then(@"l'assignation échoue avec une erreur d'article introuvable")]
    public void AlorsLassignationEchoueAvecUneErreurDarticleIntrouvable() =>
        Assert.IsType<EventItemNotFoundException>(_thrownException);

    [Then(@"l'article est assigné à ce participant")]
    public async Task AlorsLarticleEstAssigneACeParticipant()
    {
        var eventRepository = _serviceProvider.GetRequiredService<IEventRepository>();
        var @event = await eventRepository.GetByIdAsync(_existingEventId!.Value, CancellationToken.None);
        var item = @event!.Items.Single(t => t.Id == _existingItemId!.Value);

        Assert.Equal(_firstParticipantUserId!.Value, item.AssignedToUserId);
    }

    [Then(@"une notification temps réel d'assignation d'article est diffusée")]
    public void AlorsUneNotificationTempsReelDassignationDarticleEstDiffusee()
    {
        var notification = Assert.Single(_itemRealtimeNotifier.ItemAssignedNotifications);
        Assert.Equal(_existingItemId!.Value, notification.ItemId);
        Assert.Equal(_firstParticipantUserId!.Value, notification.AssignedToUserId);
    }
}
