using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Events.CreateEvent;
using EventCo.Application.Events.CreateTask;
using EventCo.Application.Events.InviteParticipant;
using EventCo.Application.Tests.Support;
using EventCo.Application.Tests.TestDoubles;
using EventCo.Domain.Events.Exceptions;
using EventCo.Infrastructure.Persistence.Repositories;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;

namespace EventCo.Application.Tests.Events.CreateTask;

[Binding]
public sealed class CreateTaskSteps
{
    private readonly IServiceProvider _serviceProvider;
    private readonly CurrentUserContext _currentUserContext;
    private readonly RecordingTaskRealtimeNotifier _taskRealtimeNotifier = new();
    private readonly DateTime _now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private Guid? _existingEventId;
    private Guid? _invitedParticipantUserId;
    private CreateTaskResult? _lastResult;
    private Exception? _thrownException;

    public CreateTaskSteps(CurrentUserContext currentUserContext)
    {
        _currentUserContext = currentUserContext;

        var builder = new ApplicationTestHostBuilder();

        builder.Services.AddScoped<IEventRepository, EventRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddSingleton<IDateTimeProvider>(new FixedDateTimeProvider(_now));
        builder.Services.AddScoped<ICurrentUserService>(_ => new CurrentUserContextService(currentUserContext));
        builder.Services.AddSingleton<ITaskRealtimeNotifier>(_taskRealtimeNotifier);

        _serviceProvider = builder.Build();
    }

    [Given(@"un événement ouvert à l'ajout de tâches ""(.*)"" prévu le ""(.*)"" au lieu ""(.*)""")]
    public async Task EtantDonneUnEvenementOuvertALajoutDeTachesPrevuLeAuLieu(string title, string eventDate, string location)
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

    [When(@"j'ajoute la tâche ""(.*)"" de catégorie ""(.*)"" et de quantité ""(.*)"" à cet événement")]
    public async Task QuandJajouteLaTacheDeCategorieEtDeQuantiteACetEvenement(string title, string category, string quantity) =>
        await AjouterTache(_existingEventId!.Value, title, category, quantity);

    [When(@"j'ajoute la tâche ""(.*)"" de catégorie ""(.*)"" et de quantité ""(.*)"" à un événement inexistant")]
    public async Task QuandJajouteLaTacheDeCategorieEtDeQuantiteAUnEvenementInexistant(string title, string category, string quantity) =>
        await AjouterTache(Guid.NewGuid(), title, category, quantity);

    private async Task AjouterTache(Guid eventId, string title, string category, string quantity)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        _thrownException = null;

        try
        {
            _lastResult = await dispatcher.Send(new CreateTaskCommand(eventId, title, category, quantity), CancellationToken.None);
        }
        catch (Exception exception)
        {
            _thrownException = exception;
        }
    }

    [Then(@"la création de la tâche réussit")]
    public void AlorsLaCreationDeLaTacheReussit() => Assert.Null(_thrownException);

    [Then(@"la création de la tâche échoue avec une erreur de validation")]
    public void AlorsLaCreationDeLaTacheEchoueAvecUneErreurDeValidation() =>
        Assert.IsType<ValidationException>(_thrownException);

    [Then(@"la création de la tâche échoue avec une erreur d'événement introuvable")]
    public void AlorsLaCreationDeLaTacheEchoueAvecUneErreurDevenementIntrouvable() =>
        Assert.IsType<EventNotFoundException>(_thrownException);

    [Then(@"la création de la tâche échoue avec une erreur d'autorisation")]
    public void AlorsLaCreationDeLaTacheEchoueAvecUneErreurDautorisation() =>
        Assert.IsType<UserNotEventParticipantException>(_thrownException);

    [Then(@"la tâche créée a pour titre ""(.*)""")]
    public void AlorsLaTacheCreeeAPourTitre(string title) => Assert.Equal(title, _lastResult!.Title);

    [Then(@"la tâche créée a pour catégorie ""(.*)""")]
    public void AlorsLaTacheCreeeAPourCategorie(string category) => Assert.Equal(category, _lastResult!.Category);

    [Then(@"une notification temps réel de création de tâche est diffusée")]
    public void AlorsUneNotificationTempsReelDeCreationDeTacheEstDiffusee()
    {
        var notification = Assert.Single(_taskRealtimeNotifier.TaskCreatedNotifications);
        Assert.Equal(_lastResult!.TaskId, notification.TaskId);
        Assert.Equal(_existingEventId!.Value, notification.EventId);
    }
}
