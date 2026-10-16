using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Events.CorrectItemCategory;
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

namespace EventCo.Application.Tests.Events.CorrectItemCategory;

[Binding]
public sealed class CorrectItemCategorySteps
{
    private readonly IServiceProvider _serviceProvider;
    private readonly CurrentUserContext _currentUserContext;
    private readonly RecordingItemRealtimeNotifier _itemRealtimeNotifier = new();
    private readonly DateTime _now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private Guid? _existingEventId;
    private Guid? _existingItemId;
    private Guid? _contributorUserId;
    private Exception? _thrownException;

    public CorrectItemCategorySteps(CurrentUserContext currentUserContext)
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

    [Given(@"un événement ""(.*)"" avec un article ""(.*)"" apporté par un participant dont je veux corriger la catégorie")]
    public async Task EtantDonneUnEvenementAvecUnArticleApporteParUnParticipantDontJeVeuxCorrigerLaCategorie(string title, string itemTitle)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        var eventCreatorUserId = _currentUserContext.UserId;

        var createResult = await dispatcher.Send(
            new CreateEventCommand(title, null, new DateTime(2026, 12, 24), "Chez Alice"),
            CancellationToken.None);
        _existingEventId = createResult.EventId;

        var inviteResult = await dispatcher.Send(
            new InviteParticipantCommand(_existingEventId.Value, "contributeur@example.com"),
            CancellationToken.None);
        _contributorUserId = inviteResult.UserId;

        _currentUserContext.UserId = _contributorUserId.Value;
        var itemResult = await dispatcher.Send(
            new CreateItemCommand(_existingEventId.Value, itemTitle, "1", "Contribution"),
            CancellationToken.None);
        _existingItemId = itemResult.ItemId;

        _currentUserContext.UserId = eventCreatorUserId;
    }

    [Given(@"je deviens la personne qui apporte cet article")]
    public void EtantDonneJeDeviensLaPersonneQuiApporteCetArticle() => _currentUserContext.UserId = _contributorUserId!.Value;

    [Given(@"je deviens un autre participant de cet événement")]
    public async Task EtantDonneJeDeviensUnAutreParticipantDeCetEvenement()
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        var inviteResult = await dispatcher.Send(
            new InviteParticipantCommand(_existingEventId!.Value, "autre@example.com"),
            CancellationToken.None);

        _currentUserContext.UserId = inviteResult.UserId;
    }

    [When(@"je corrige la catégorie de cet article en ""(.*)""")]
    public async Task QuandJeCorrigeLaCategorieDeCetArticleEn(string category) =>
        await Corriger(_existingEventId!.Value, _existingItemId!.Value, category);

    [When(@"je corrige la catégorie d'un article sur un événement inexistant")]
    public async Task QuandJeCorrigeLaCategorieDunArticleSurUnEvenementInexistant() =>
        await Corriger(Guid.NewGuid(), Guid.NewGuid(), "Desserts");

    [When(@"je corrige la catégorie d'un article inexistant sur cet événement")]
    public async Task QuandJeCorrigeLaCategorieDunArticleInexistantSurCetEvenement() =>
        await Corriger(_existingEventId!.Value, Guid.NewGuid(), "Desserts");

    private async Task Corriger(Guid eventId, Guid itemId, string category)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        _thrownException = null;

        try
        {
            await dispatcher.Send(new CorrectItemCategoryCommand(eventId, itemId, category), CancellationToken.None);
        }
        catch (Exception exception)
        {
            _thrownException = exception;
        }
    }

    [Then(@"la correction de catégorie réussit")]
    public void AlorsLaCorrectionDeCategorieReussit() => Assert.Null(_thrownException);

    [Then(@"la catégorie de cet article est ""(.*)""")]
    public async Task AlorsLaCategorieDeCetArticleEst(string category)
    {
        var eventRepository = _serviceProvider.GetRequiredService<IEventRepository>();
        var @event = await eventRepository.GetByIdAsync(_existingEventId!.Value, CancellationToken.None);
        var item = @event!.Items.Single(t => t.Id == _existingItemId!.Value);

        Assert.Equal(category, item.Category.ToString());
    }

    [Then(@"une notification temps réel de correction de catégorie d'article est diffusée")]
    public void AlorsUneNotificationTempsReelDeCorrectionDeCategorieDarticleEstDiffusee()
    {
        var notification = Assert.Single(_itemRealtimeNotifier.ItemCategoryCorrectedNotifications);
        Assert.Equal(_existingItemId!.Value, notification.ItemId);
        Assert.Equal("Desserts", notification.Category);
    }

    [Then(@"la correction de catégorie échoue avec une erreur de correction réservée au créateur de l'article et aux organisateurs")]
    public void AlorsLaCorrectionDeCategorieEchoueAvecUneErreurDeCorrectionReservee() =>
        Assert.IsType<ParticipantCannotCorrectOthersItemCategoryException>(_thrownException);

    [Then(@"la correction de catégorie échoue avec une erreur d'autorisation")]
    public void AlorsLaCorrectionDeCategorieEchoueAvecUneErreurDautorisation() =>
        Assert.IsType<UserNotEventParticipantException>(_thrownException);

    [Then(@"la correction de catégorie échoue avec une erreur de validation")]
    public void AlorsLaCorrectionDeCategorieEchoueAvecUneErreurDeValidation() =>
        Assert.IsType<ValidationException>(_thrownException);

    [Then(@"la correction de catégorie échoue avec une erreur d'événement introuvable")]
    public void AlorsLaCorrectionDeCategorieEchoueAvecUneErreurDevenementIntrouvable() =>
        Assert.IsType<EventNotFoundException>(_thrownException);

    [Then(@"la correction de catégorie échoue avec une erreur d'article introuvable")]
    public void AlorsLaCorrectionDeCategorieEchoueAvecUneErreurDarticleIntrouvable() =>
        Assert.IsType<EventItemNotFoundException>(_thrownException);
}
