using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Events.AssignTask;
using EventCo.Application.Events.CreateEvent;
using EventCo.Application.Events.CreateTask;
using EventCo.Application.Events.InviteParticipant;
using EventCo.Application.Events.UnassignTask;
using EventCo.Application.Tests.Support;
using EventCo.Application.Tests.TestDoubles;
using EventCo.Domain.Events;
using EventCo.Domain.Events.Exceptions;
using EventCo.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;

namespace EventCo.Application.Tests.Events.UnassignTask;

[Binding]
public sealed class UnassignTaskSteps
{
    private readonly IServiceProvider _serviceProvider;
    private readonly CurrentUserContext _currentUserContext;
    private readonly RecordingTaskRealtimeNotifier _taskRealtimeNotifier = new();
    private readonly DateTime _now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private Guid? _existingEventId;
    private Guid? _existingTaskId;
    private Guid? _assignedParticipantUserId;
    private Exception? _thrownException;

    public UnassignTaskSteps(CurrentUserContext currentUserContext)
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

    [Given(@"un événement ""(.*)"" avec une tâche ""(.*)"" déjà assignée à un participant, prévu le ""(.*)"" au lieu ""(.*)""")]
    public async Task EtantDonneUnEvenementAvecUneTacheDejaAssigneeAUnParticipantPrevuLeAuLieu(string title, string taskTitle, string eventDate, string location)
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

        var inviteResult = await dispatcher.Send(
            new InviteParticipantCommand(_existingEventId.Value, "assignee@example.com"),
            CancellationToken.None);
        _assignedParticipantUserId = inviteResult.UserId;

        await dispatcher.Send(
            new AssignTaskCommand(_existingEventId.Value, _existingTaskId.Value, _assignedParticipantUserId.Value),
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

    [When(@"je désassigne cette tâche")]
    public async Task QuandJeDesassigneCetteTache() =>
        await Desassigner(_existingEventId!.Value, _existingTaskId!.Value);

    [When(@"je désassigne une tâche sur un événement inexistant")]
    public async Task QuandJeDesassigneUneTacheSurUnEvenementInexistant() =>
        await Desassigner(Guid.NewGuid(), Guid.NewGuid());

    private async Task Desassigner(Guid eventId, Guid taskId)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        _thrownException = null;

        try
        {
            await dispatcher.Send(new UnassignTaskCommand(eventId, taskId), CancellationToken.None);
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
        Assert.IsType<ParticipantCannotUnassignOthersTaskException>(_thrownException);

    [Then(@"la désassignation échoue avec une erreur d'autorisation")]
    public void AlorsLaDesassignationEchoueAvecUneErreurDautorisation() =>
        Assert.IsType<UserNotEventParticipantException>(_thrownException);

    [Then(@"la désassignation échoue avec une erreur d'événement introuvable")]
    public void AlorsLaDesassignationEchoueAvecUneErreurDevenementIntrouvable() =>
        Assert.IsType<EventNotFoundException>(_thrownException);

    [Then(@"la tâche n'est plus assignée")]
    public async Task AlorsLaTacheNestPlusAssignee()
    {
        var eventRepository = _serviceProvider.GetRequiredService<IEventRepository>();
        var @event = await eventRepository.GetByIdAsync(_existingEventId!.Value, CancellationToken.None);
        var task = @event!.Tasks.Single(t => t.Id == _existingTaskId!.Value);

        Assert.Null(task.AssignedToUserId);
    }

    [Then(@"une notification temps réel de désassignation de tâche est diffusée")]
    public void AlorsUneNotificationTempsReelDeDesassignationDeTacheEstDiffusee()
    {
        var notification = Assert.Single(_taskRealtimeNotifier.TaskUnassignedNotifications);
        Assert.Equal(_existingTaskId!.Value, notification.TaskId);
        Assert.Null(notification.AssignedToUserId);
    }
}
