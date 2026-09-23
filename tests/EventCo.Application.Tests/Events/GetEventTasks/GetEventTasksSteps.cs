using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Events.CreateEvent;
using EventCo.Application.Events.CreateTask;
using EventCo.Application.Events.GetEventTasks;
using EventCo.Application.Tests.Support;
using EventCo.Application.Tests.TestDoubles;
using EventCo.Domain.Events.Exceptions;
using EventCo.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;

namespace EventCo.Application.Tests.Events.GetEventTasks;

[Binding]
public sealed class GetEventTasksSteps
{
    private readonly IServiceProvider _serviceProvider;
    private readonly CurrentUserContext _currentUserContext;
    private readonly DateTime _now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private Guid? _existingEventId;
    private GetEventTasksResult? _lastResult;
    private Exception? _thrownException;

    public GetEventTasksSteps(CurrentUserContext currentUserContext)
    {
        _currentUserContext = currentUserContext;

        var builder = new ApplicationTestHostBuilder();

        builder.Services.AddScoped<IEventRepository, EventRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddSingleton<IDateTimeProvider>(new FixedDateTimeProvider(_now));
        builder.Services.AddScoped<ICurrentUserService>(_ => new CurrentUserContextService(currentUserContext));
        builder.Services.AddSingleton<ITaskRealtimeNotifier>(new RecordingTaskRealtimeNotifier());

        _serviceProvider = builder.Build();
    }

    [Given(@"un événement ""(.*)"" prévu le ""(.*)"" au lieu ""(.*)"" dont je veux consulter les tâches")]
    public async Task EtantDonneUnEvenementDontJeVeuxConsulterLesTaches(string title, string eventDate, string location)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        var createResult = await dispatcher.Send(
            new CreateEventCommand(title, null, DateTime.Parse(eventDate), location),
            CancellationToken.None);

        _existingEventId = createResult.EventId;
    }

    [Given(@"une tâche ""(.*)"" de catégorie ""(.*)"" est ajoutée à cet événement")]
    public async Task EtantDonneUneTacheDeCategorieEstAjouteeACetEvenement(string title, string category)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        await dispatcher.Send(new CreateTaskCommand(_existingEventId!.Value, title, category, null), CancellationToken.None);
    }

    [When(@"je consulte les tâches de cet événement")]
    public async Task QuandJeConsulteLesTachesDeCetEvenement() => await ConsulterLesTaches(_existingEventId!.Value);

    [Given(@"je change d'utilisateur courant pour un administrateur")]
    [Scope(Feature = "Consultation des tâches d'un événement")]
    public async Task EtantDonneJeChangeDutilisateurCourantPourUnAdministrateur() =>
        _currentUserContext.UserId = await AdminUserSeeder.SeedAsync(_serviceProvider);

    [When(@"je consulte les tâches d'un événement inexistant")]
    public async Task QuandJeConsulteLesTachesDunEvenementInexistant() => await ConsulterLesTaches(Guid.NewGuid());

    private async Task ConsulterLesTaches(Guid eventId)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        try
        {
            _lastResult = await dispatcher.Send(new GetEventTasksQuery(eventId), CancellationToken.None);
        }
        catch (Exception exception)
        {
            _thrownException = exception;
        }
    }

    [Then(@"la consultation des tâches réussit")]
    public void AlorsLaConsultationDesTachesReussit() => Assert.Null(_thrownException);

    [Then(@"la consultation des tâches échoue avec une erreur d'événement introuvable")]
    public void AlorsLaConsultationDesTachesEchoueAvecUneErreurDevenementIntrouvable() =>
        Assert.IsType<EventNotFoundException>(_thrownException);

    [Then(@"la consultation des tâches échoue avec une erreur d'autorisation")]
    public void AlorsLaConsultationDesTachesEchoueAvecUneErreurDautorisation() =>
        Assert.IsType<UserNotEventParticipantException>(_thrownException);

    [Then(@"l'événement consulté a (\d+) tâches?")]
    public void AlorsLevenementConsulteATaches(int count) => Assert.Equal(count, _lastResult!.Tasks.Count);

    [Then(@"l'événement consulté a une tâche ""(.*)"" de catégorie ""(.*)""")]
    public void AlorsLevenementConsulteAUneTacheDeCategorie(string title, string category)
    {
        var task = Assert.Single(_lastResult!.Tasks, t => t.Title == title);
        Assert.Equal(category, task.Category);
    }
}
