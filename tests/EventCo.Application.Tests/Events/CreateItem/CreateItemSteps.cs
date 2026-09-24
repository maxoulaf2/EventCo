using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Events.CreateEvent;
using EventCo.Application.Events.CreateItem;
using EventCo.Application.Events.InviteParticipant;
using EventCo.Application.Tests.Support;
using EventCo.Application.Tests.TestDoubles;
using EventCo.Domain.Events.Exceptions;
using EventCo.Infrastructure.Persistence.Repositories;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;

namespace EventCo.Application.Tests.Events.CreateItem;

[Binding]
public sealed class CreateItemSteps
{
    private readonly IServiceProvider _serviceProvider;
    private readonly CurrentUserContext _currentUserContext;
    private readonly RecordingItemRealtimeNotifier _itemRealtimeNotifier = new();
    private readonly DateTime _now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private Guid? _existingEventId;
    private Guid? _invitedParticipantUserId;
    private CreateItemResult? _lastResult;
    private Exception? _thrownException;

    public CreateItemSteps(CurrentUserContext currentUserContext)
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

    [Given(@"un événement ouvert à l'ajout d'articles ""(.*)"" prévu le ""(.*)"" au lieu ""(.*)""")]
    public async Task EtantDonneUnEvenementOuvertALajoutDarticlesPrevuLeAuLieu(string title, string eventDate, string location)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        var createResult = await dispatcher.Send(
            new CreateEventCommand(title, null, DateTime.Parse(eventDate), location),
            CancellationToken.None);

        _existingEventId = createResult.EventId;
    }

    [Given(@"un participant ""(.*)"" a rejoint cet événement")]
    public async Task EtantDonneUnParticipantARejointCetEvenement(string email)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        var inviteResult = await dispatcher.Send(
            new InviteParticipantCommand(_existingEventId!.Value, email),
            CancellationToken.None);
        _invitedParticipantUserId = inviteResult.UserId;
    }

    [Given(@"j'agis désormais en tant que ce participant")]
    public void EtantDonneJagisDesormaisEnTantQueCeParticipant() => _currentUserContext.UserId = _invitedParticipantUserId!.Value;

    [When(@"j'ajoute l'article ""(.*)"" de quantité ""(.*)"" à cet événement")]
    public async Task QuandJajouteLarticleDeCategorieEtDeQuantiteACetEvenement(string title, string quantity) =>
        await AjouterArticle(_existingEventId!.Value, title, quantity);

    [When(@"j'ajoute l'article ""(.*)"" de quantité ""(.*)"" à un événement inexistant")]
    public async Task QuandJajouteLarticleDeCategorieEtDeQuantiteAUnEvenementInexistant(string title, string quantity) =>
        await AjouterArticle(Guid.NewGuid(), title, quantity);

    private async Task AjouterArticle(Guid eventId, string title, string quantity)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        _thrownException = null;

        try
        {
            _lastResult = await dispatcher.Send(new CreateItemCommand(eventId, title, quantity), CancellationToken.None);
        }
        catch (Exception exception)
        {
            _thrownException = exception;
        }
    }

    [Then(@"la création de l'article réussit")]
    public void AlorsLaCreationDeLarticleReussit() => Assert.Null(_thrownException);

    [Then(@"la création de l'article échoue avec une erreur de validation")]
    public void AlorsLaCreationDeLarticleEchoueAvecUneErreurDeValidation() =>
        Assert.IsType<ValidationException>(_thrownException);

    [Then(@"la création de l'article échoue avec une erreur d'événement introuvable")]
    public void AlorsLaCreationDeLarticleEchoueAvecUneErreurDevenementIntrouvable() =>
        Assert.IsType<EventNotFoundException>(_thrownException);

    [Then(@"la création de l'article échoue avec une erreur d'autorisation")]
    public void AlorsLaCreationDeLarticleEchoueAvecUneErreurDautorisation() =>
        Assert.IsType<UserNotEventParticipantException>(_thrownException);

    [Then(@"l'article créé a pour titre ""(.*)""")]
    public void AlorsLarticleCreeAPourTitre(string title) => Assert.Equal(title, _lastResult!.Title);

    [Then(@"une notification temps réel de création d'article est diffusée")]
    public void AlorsUneNotificationTempsReelDeCreationDarticleEstDiffusee()
    {
        var notification = Assert.Single(_itemRealtimeNotifier.ItemCreatedNotifications);
        Assert.Equal(_lastResult!.ItemId, notification.ItemId);
        Assert.Equal(_existingEventId!.Value, notification.EventId);
    }
}
