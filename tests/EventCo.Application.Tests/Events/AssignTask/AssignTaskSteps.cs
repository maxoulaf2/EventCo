using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Events.AssignTask;
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

namespace EventCo.Application.Tests.Events.AssignTask;

[Binding]
public sealed class AssignTaskSteps
{
    private readonly IServiceProvider _serviceProvider;
    private readonly CurrentUserContext _currentUserContext;
    private readonly RecordingTaskRealtimeNotifier _taskRealtimeNotifier = new();
    private readonly DateTime _now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private Guid? _existingEventId;
    private Guid? _existingTaskId;
    private Guid? _firstParticipantUserId;
    private Guid? _secondParticipantUserId;
    private Exception? _thrownException;

    public AssignTaskSteps(CurrentUserContext currentUserContext)
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

    [Given(@"un événement ""(.*)"" avec une tâche ""(.*)"", prévu le ""(.*)"" au lieu ""(.*)""")]
    public async Task EtantDonneUnEvenementAvecUneTachePrevuLeAuLieu(string title, string taskTitle, string eventDate, string location)
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

    [When(@"j'assigne cette tâche à moi-même")]
    public async Task QuandJassigneCetteTacheAMoiMeme() =>
        await Assigner(_existingEventId!.Value, _existingTaskId!.Value, _currentUserContext.UserId);

    [When(@"j'assigne cette tâche à ce participant")]
    public async Task QuandJassigneCetteTacheACeParticipant() =>
        await Assigner(_existingEventId!.Value, _existingTaskId!.Value, _firstParticipantUserId!.Value);

    [When(@"j'assigne cette tâche à l'autre participant")]
    public async Task QuandJassigneCetteTacheALautreParticipant() =>
        await Assigner(_existingEventId!.Value, _existingTaskId!.Value, _secondParticipantUserId!.Value);

    [When(@"j'assigne cette tâche à un utilisateur qui n'est pas participant")]
    public async Task QuandJassigneCetteTacheAUnUtilisateurQuiNestPasParticipant() =>
        await Assigner(_existingEventId!.Value, _existingTaskId!.Value, Guid.NewGuid());

    [When(@"j'assigne une tâche à un événement inexistant")]
    public async Task QuandJassigneUneTacheAUnEvenementInexistant() =>
        await Assigner(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

    private async Task Assigner(Guid eventId, Guid taskId, Guid userId)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        _thrownException = null;

        try
        {
            await dispatcher.Send(new AssignTaskCommand(eventId, taskId, userId), CancellationToken.None);
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
        Assert.IsType<ParticipantCannotAssignTaskToOthersException>(_thrownException);

    [Then(@"l'assignation échoue avec une erreur d'assigné invalide")]
    public void AlorsLassignationEchoueAvecUneErreurDassigneInvalide() =>
        Assert.IsType<TaskAssigneeNotParticipantException>(_thrownException);

    [Then(@"l'assignation échoue avec une erreur d'autorisation")]
    public void AlorsLassignationEchoueAvecUneErreurDautorisation() =>
        Assert.IsType<UserNotEventParticipantException>(_thrownException);

    [Then(@"l'assignation échoue avec une erreur d'événement introuvable")]
    public void AlorsLassignationEchoueAvecUneErreurDevenementIntrouvable() =>
        Assert.IsType<EventNotFoundException>(_thrownException);

    [Then(@"la tâche est assignée à ce participant")]
    public async Task AlorsLaTacheEstAssigneeACeParticipant()
    {
        var eventRepository = _serviceProvider.GetRequiredService<IEventRepository>();
        var @event = await eventRepository.GetByIdAsync(_existingEventId!.Value, CancellationToken.None);
        var task = @event!.Tasks.Single(t => t.Id == _existingTaskId!.Value);

        Assert.Equal(_firstParticipantUserId!.Value, task.AssignedToUserId);
    }

    [Then(@"une notification temps réel d'assignation de tâche est diffusée")]
    public void AlorsUneNotificationTempsReelDassignationDeTacheEstDiffusee()
    {
        var notification = Assert.Single(_taskRealtimeNotifier.TaskAssignedNotifications);
        Assert.Equal(_existingTaskId!.Value, notification.TaskId);
        Assert.Equal(_firstParticipantUserId!.Value, notification.AssignedToUserId);
    }
}
