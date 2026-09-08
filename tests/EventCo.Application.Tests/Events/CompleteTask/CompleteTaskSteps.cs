using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Events.AssignTask;
using EventCo.Application.Events.CompleteTask;
using EventCo.Application.Events.CreateEvent;
using EventCo.Application.Events.CreateTask;
using EventCo.Application.Events.InviteParticipant;
using EventCo.Application.Tests.Support;
using EventCo.Application.Tests.TestDoubles;
using EventCo.Domain.Events;
using EventCo.Domain.Events.Exceptions;
using EventCo.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;

namespace EventCo.Application.Tests.Events.CompleteTask;

[Binding]
public sealed class CompleteTaskSteps
{
    private readonly IServiceProvider _serviceProvider;
    private readonly CurrentUserContext _currentUserContext;
    private readonly RecordingTaskRealtimeNotifier _taskRealtimeNotifier = new();
    private readonly DateTime _now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private Guid? _existingEventId;
    private Guid? _existingTaskId;
    private Guid? _assignedParticipantUserId;
    private Exception? _thrownException;

    public CompleteTaskSteps(CurrentUserContext currentUserContext)
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

    [Given(@"un événement ""(.*)"" avec une tâche assignée ""(.*)"", prévu le ""(.*)"" au lieu ""(.*)""")]
    public async Task EtantDonneUnEvenementAvecUneTacheAssigneePrevuLeAuLieu(string title, string taskTitle, string eventDate, string location)
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
    }

    [Given(@"un événement ""(.*)"" avec une tâche non assignée ""(.*)"", prévu le ""(.*)"" au lieu ""(.*)""")]
    public async Task EtantDonneUnEvenementAvecUneTacheNonAssigneePrevuLeAuLieu(string title, string taskTitle, string eventDate, string location) =>
        await CreerEvenementEtTache(title, taskTitle, eventDate, location);

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

    [Given(@"je deviens le participant assigné à cette tâche")]
    public void EtantDonneJeDeviensLeParticipantAssigneACetteTache() =>
        _currentUserContext.UserId = _assignedParticipantUserId!.Value;

    [Given(@"un autre participant a rejoint cet événement et devient l'utilisateur courant")]
    public async Task EtantDonneUnAutreParticipantARejointCetEvenementEtDevientLutilisateurCourant()
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        var inviteResult = await dispatcher.Send(
            new InviteParticipantCommand(_existingEventId!.Value, "autre@example.com"),
            CancellationToken.None);

        _currentUserContext.UserId = inviteResult.UserId;
    }

    [When(@"je marque cette tâche comme faite")]
    public async Task QuandJeMarqueCetteTacheCommeFaite() =>
        await Marquer(_existingEventId!.Value, _existingTaskId!.Value);

    [When(@"je marque une tâche comme faite sur un événement inexistant")]
    public async Task QuandJeMarqueUneTacheCommeFaiteSurUnEvenementInexistant() =>
        await Marquer(Guid.NewGuid(), Guid.NewGuid());

    private async Task Marquer(Guid eventId, Guid taskId)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        _thrownException = null;

        try
        {
            await dispatcher.Send(new CompleteTaskCommand(eventId, taskId), CancellationToken.None);
        }
        catch (Exception exception)
        {
            _thrownException = exception;
        }
    }

    [Then(@"le marquage réussit")]
    public void AlorsLeMarquageReussit() => Assert.Null(_thrownException);

    [Then(@"le marquage échoue avec une erreur de statut réservé à l'assigné")]
    public void AlorsLeMarquageEchoueAvecUneErreurDeStatutReserveALassigne() =>
        Assert.IsType<ParticipantCannotToggleOthersTaskException>(_thrownException);

    [Then(@"le marquage échoue avec une erreur d'autorisation")]
    public void AlorsLeMarquageEchoueAvecUneErreurDautorisation() =>
        Assert.IsType<UserNotEventParticipantException>(_thrownException);

    [Then(@"le marquage échoue avec une erreur d'événement introuvable")]
    public void AlorsLeMarquageEchoueAvecUneErreurDevenementIntrouvable() =>
        Assert.IsType<EventNotFoundException>(_thrownException);

    [Then(@"la tâche est marquée comme faite")]
    public async Task AlorsLaTacheEstMarqueeCommeFaite()
    {
        var eventRepository = _serviceProvider.GetRequiredService<IEventRepository>();
        var @event = await eventRepository.GetByIdAsync(_existingEventId!.Value, CancellationToken.None);
        var task = @event!.Tasks.Single(t => t.Id == _existingTaskId!.Value);

        Assert.True(task.IsDone);
    }

    [Then(@"une notification temps réel de tâche marquée comme faite est diffusée")]
    public void AlorsUneNotificationTempsReelDeTacheMarqueeCommeFaiteEstDiffusee()
    {
        var notification = Assert.Single(_taskRealtimeNotifier.TaskStatusChangedNotifications);
        Assert.Equal(_existingTaskId!.Value, notification.TaskId);
        Assert.True(notification.IsDone);
    }
}
