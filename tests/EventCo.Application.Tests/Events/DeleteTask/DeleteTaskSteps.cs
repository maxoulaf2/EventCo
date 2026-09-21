using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Events.CreateEvent;
using EventCo.Application.Events.CreateTask;
using EventCo.Application.Events.DeleteTask;
using EventCo.Application.Events.InviteParticipant;
using EventCo.Application.Tests.Support;
using EventCo.Application.Tests.TestDoubles;
using EventCo.Domain.Events;
using EventCo.Domain.Events.Exceptions;
using EventCo.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;

namespace EventCo.Application.Tests.Events.DeleteTask;

[Binding]
public sealed class DeleteTaskSteps
{
    private readonly IServiceProvider _serviceProvider;
    private readonly CurrentUserContext _currentUserContext;
    private readonly RecordingTaskRealtimeNotifier _taskRealtimeNotifier = new();
    private readonly DateTime _now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private Guid? _existingEventId;
    private Guid? _existingTaskId;
    private Guid? _eventCreatorUserId;
    private Guid? _taskCreatorUserId;
    private Guid? _otherInvitedUserId;
    private Exception? _thrownException;

    public DeleteTaskSteps(CurrentUserContext currentUserContext)
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

    [Given(@"un événement ""(.*)"" avec une tâche créée par un participant invité ""(.*)"", prévu le ""(.*)"" au lieu ""(.*)""")]
    public async Task EtantDonneUnEvenementAvecUneTacheCreeeParUnParticipantInvitePrevuLeAuLieu(string title, string taskTitle, string eventDate, string location)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        _eventCreatorUserId = _currentUserContext.UserId;

        var createResult = await dispatcher.Send(
            new CreateEventCommand(title, null, DateTime.Parse(eventDate), location),
            CancellationToken.None);
        _existingEventId = createResult.EventId;

        var taskCreatorInviteResult = await dispatcher.Send(
            new InviteParticipantCommand(_existingEventId.Value, "createur-tache@example.com"),
            CancellationToken.None);
        _taskCreatorUserId = taskCreatorInviteResult.UserId;

        var otherInviteResult = await dispatcher.Send(
            new InviteParticipantCommand(_existingEventId.Value, "autre-participant@example.com"),
            CancellationToken.None);
        _otherInvitedUserId = otherInviteResult.UserId;

        _currentUserContext.UserId = _taskCreatorUserId.Value;

        var taskResult = await dispatcher.Send(
            new CreateTaskCommand(_existingEventId.Value, taskTitle, nameof(TaskCategory.Courses), "1"),
            CancellationToken.None);
        _existingTaskId = taskResult.TaskId;
    }

    [Given(@"je redeviens le créateur de l'événement")]
    public void EtantDonneJeRedeviensLeCreateurDeLevenement() =>
        _currentUserContext.UserId = _eventCreatorUserId!.Value;

    [Given(@"un autre participant invité devient l'utilisateur courant")]
    public void EtantDonneUnAutreParticipantInviteDevientLutilisateurCourant() =>
        _currentUserContext.UserId = _otherInvitedUserId!.Value;

    [When(@"je supprime cette tâche")]
    public async Task QuandJeSupprimeCetteTache() =>
        await Supprimer(_existingEventId!.Value, _existingTaskId!.Value);

    [When(@"je supprime une tâche sur un événement inexistant")]
    public async Task QuandJeSupprimeUneTacheSurUnEvenementInexistant() =>
        await Supprimer(Guid.NewGuid(), Guid.NewGuid());

    [When(@"je supprime une tâche inexistante sur cet événement")]
    public async Task QuandJeSupprimeUneTacheInexistanteSurCetEvenement() =>
        await Supprimer(_existingEventId!.Value, Guid.NewGuid());

    private async Task Supprimer(Guid eventId, Guid taskId)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        _thrownException = null;

        try
        {
            await dispatcher.Send(new DeleteTaskCommand(eventId, taskId), CancellationToken.None);
        }
        catch (Exception exception)
        {
            _thrownException = exception;
        }
    }

    [Then(@"la suppression de la tâche réussit")]
    public void AlorsLaSuppressionDeLaTacheReussit() => Assert.Null(_thrownException);

    [Then(@"la suppression de la tâche échoue avec une erreur de suppression réservée au créateur de la tâche")]
    public void AlorsLaSuppressionDeLaTacheEchoueAvecUneErreurDeSuppressionReserveeAuCreateurDeLaTache() =>
        Assert.IsType<ParticipantCannotDeleteOthersTaskException>(_thrownException);

    [Then(@"la suppression de la tâche échoue avec une erreur d'autorisation")]
    public void AlorsLaSuppressionDeLaTacheEchoueAvecUneErreurDautorisation() =>
        Assert.IsType<UserNotEventParticipantException>(_thrownException);

    [Then(@"la suppression de la tâche échoue avec une erreur d'événement introuvable")]
    public void AlorsLaSuppressionDeLaTacheEchoueAvecUneErreurDevenementIntrouvable() =>
        Assert.IsType<EventNotFoundException>(_thrownException);

    [Then(@"la suppression de la tâche échoue avec une erreur de tâche introuvable")]
    public void AlorsLaSuppressionDeLaTacheEchoueAvecUneErreurDeTacheIntrouvable() =>
        Assert.IsType<EventTaskNotFoundException>(_thrownException);

    [Then(@"la tâche n'existe plus")]
    public async Task AlorsLaTacheNexistePlus()
    {
        var eventRepository = _serviceProvider.GetRequiredService<IEventRepository>();
        var @event = await eventRepository.GetByIdAsync(_existingEventId!.Value, CancellationToken.None);

        Assert.DoesNotContain(@event!.Tasks, t => t.Id == _existingTaskId!.Value);
    }

    [Then(@"une notification temps réel de suppression de tâche est diffusée")]
    public void AlorsUneNotificationTempsReelDeSuppressionDeTacheEstDiffusee()
    {
        var notification = Assert.Single(_taskRealtimeNotifier.TaskDeletedNotifications);
        Assert.Equal(_existingTaskId!.Value, notification.TaskId);
        Assert.Equal(_existingEventId!.Value, notification.EventId);
    }
}
