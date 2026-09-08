using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Events.AssignTask;
using EventCo.Application.Events.CompleteTask;
using EventCo.Application.Events.CreateEvent;
using EventCo.Application.Events.CreateTask;
using EventCo.Application.Events.InviteParticipant;
using EventCo.Application.Events.ReopenTask;
using EventCo.Application.Tests.Support;
using EventCo.Application.Tests.TestDoubles;
using EventCo.Domain.Events;
using EventCo.Domain.Events.Exceptions;
using EventCo.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;

namespace EventCo.Application.Tests.Events.ReopenTask;

[Binding]
public sealed class ReopenTaskSteps
{
    private readonly IServiceProvider _serviceProvider;
    private readonly CurrentUserContext _currentUserContext;
    private readonly RecordingTaskRealtimeNotifier _taskRealtimeNotifier = new();
    private readonly DateTime _now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private Guid? _existingEventId;
    private Guid? _existingTaskId;
    private Guid? _assignedParticipantUserId;
    private Exception? _thrownException;

    public ReopenTaskSteps(CurrentUserContext currentUserContext)
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

    [Given(@"un événement ""(.*)"" avec une tâche assignée déjà marquée comme faite ""(.*)"", prévu le ""(.*)"" au lieu ""(.*)""")]
    public async Task EtantDonneUnEvenementAvecUneTacheAssigneeDejaMarqueeCommeFaitePrevuLeAuLieu(string title, string taskTitle, string eventDate, string location)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        await CreerEvenementEtTache(title, taskTitle, eventDate, location);

        var inviteResult = await dispatcher.Send(
            new InviteParticipantCommand(_existingEventId!.Value, "assignee@example.com"),
            CancellationToken.None);
        _assignedParticipantUserId = inviteResult.UserId;

        await dispatcher.Send(
            new AssignTaskCommand(_existingEventId.Value, _existingTaskId!.Value, _assignedParticipantUserId.Value),
            CancellationToken.None);

        await dispatcher.Send(new CompleteTaskCommand(_existingEventId.Value, _existingTaskId.Value), CancellationToken.None);
    }

    [Given(@"un événement ""(.*)"" avec une tâche non assignée déjà marquée comme faite ""(.*)"", prévu le ""(.*)"" au lieu ""(.*)""")]
    public async Task EtantDonneUnEvenementAvecUneTacheNonAssigneeDejaMarqueeCommeFaitePrevuLeAuLieu(string title, string taskTitle, string eventDate, string location)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        await CreerEvenementEtTache(title, taskTitle, eventDate, location);

        await dispatcher.Send(new CompleteTaskCommand(_existingEventId!.Value, _existingTaskId!.Value), CancellationToken.None);
    }

    private async Task CreerEvenementEtTache(string title, string taskTitle, string eventDate, string location)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        var createResult = await dispatcher.Send(
            new CreateEventCommand(title, null, DateTime.Parse(eventDate), location),
            CancellationToken.None);
        _existingEventId = createResult.EventId;

        var taskResult = await dispatcher.Send(
            new CreateTaskCommand(_existingEventId.Value, taskTitle, nameof(TaskCategory.Courses), "1"),
            CancellationToken.None);
        _existingTaskId = taskResult.TaskId;
    }

    [Given(@"je suis le participant auquel cette tâche est assignée")]
    public void EtantDonneJeSuisLeParticipantAuquelCetteTacheEstAssignee() =>
        _currentUserContext.UserId = _assignedParticipantUserId!.Value;

    [Given(@"un second participant a également rejoint cet événement et en devient l'utilisateur courant")]
    public async Task EtantDonneUnSecondParticipantAEgalementRejointCetEvenementEtEnDevientLutilisateurCourant()
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        var inviteResult = await dispatcher.Send(
            new InviteParticipantCommand(_existingEventId!.Value, "autre@example.com"),
            CancellationToken.None);

        _currentUserContext.UserId = inviteResult.UserId;
    }

    [When(@"je marque cette tâche comme non faite")]
    public async Task QuandJeMarqueCetteTacheCommeNonFaite() =>
        await Rouvrir(_existingEventId!.Value, _existingTaskId!.Value);

    [When(@"je marque une tâche comme non faite sur un événement inexistant")]
    public async Task QuandJeMarqueUneTacheCommeNonFaiteSurUnEvenementInexistant() =>
        await Rouvrir(Guid.NewGuid(), Guid.NewGuid());

    private async Task Rouvrir(Guid eventId, Guid taskId)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        _thrownException = null;

        try
        {
            await dispatcher.Send(new ReopenTaskCommand(eventId, taskId), CancellationToken.None);
        }
        catch (Exception exception)
        {
            _thrownException = exception;
        }
    }

    [Then(@"la réouverture réussit")]
    public void AlorsLaReouvertureReussit() => Assert.Null(_thrownException);

    [Then(@"la réouverture échoue avec une erreur de statut réservé à l'assigné")]
    public void AlorsLaReouvertureEchoueAvecUneErreurDeStatutReserveALassigne() =>
        Assert.IsType<ParticipantCannotToggleOthersTaskException>(_thrownException);

    [Then(@"la réouverture échoue avec une erreur d'autorisation")]
    public void AlorsLaReouvertureEchoueAvecUneErreurDautorisation() =>
        Assert.IsType<UserNotEventParticipantException>(_thrownException);

    [Then(@"la réouverture échoue avec une erreur d'événement introuvable")]
    public void AlorsLaReouvertureEchoueAvecUneErreurDevenementIntrouvable() =>
        Assert.IsType<EventNotFoundException>(_thrownException);

    [Then(@"la tâche est marquée comme non faite")]
    public async Task AlorsLaTacheEstMarqueeCommeNonFaite()
    {
        var eventRepository = _serviceProvider.GetRequiredService<IEventRepository>();
        var @event = await eventRepository.GetByIdAsync(_existingEventId!.Value, CancellationToken.None);
        var task = @event!.Tasks.Single(t => t.Id == _existingTaskId!.Value);

        Assert.False(task.IsDone);
    }

    [Then(@"une notification temps réel de tâche marquée comme non faite est diffusée")]
    public void AlorsUneNotificationTempsReelDeTacheMarqueeCommeNonFaiteEstDiffusee()
    {
        // Le Given de ce scénario marque déjà la tâche comme faite (CompleteTaskCommand), ce qui émet
        // une première notification : seule la dernière correspond à la réouverture testée par ce step.
        var notification = Assert.Single(_taskRealtimeNotifier.TaskStatusChangedNotifications, n => !n.IsDone);
        Assert.Equal(_existingTaskId!.Value, notification.TaskId);
    }
}
