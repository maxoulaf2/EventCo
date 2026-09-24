using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Events.GetAllEvents;
using EventCo.Application.Tests.Support;
using EventCo.Application.Tests.TestDoubles;
using EventCo.Domain.Events;
using EventCo.Domain.Users.Exceptions;
using EventCo.Infrastructure.Persistence;
using EventCo.Infrastructure.Persistence.Mapping;
using EventCo.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;

namespace EventCo.Application.Tests.Events.GetAllEvents;

[Binding]
public sealed class GetAllEventsSteps
{
    private readonly IServiceProvider _serviceProvider;
    private readonly EventCoDbContext _dbContext;
    private readonly CurrentUserContext _currentUserContext;
    private readonly DateTime _now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private GetAllEventsResult? _lastResult;
    private Exception? _thrownException;

    public GetAllEventsSteps(CurrentUserContext currentUserContext)
    {
        _currentUserContext = currentUserContext;

        var builder = new ApplicationTestHostBuilder();

        builder.Services.AddScoped<IEventRepository, EventRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddSingleton<IDateTimeProvider>(new FixedDateTimeProvider(_now));
        builder.Services.AddScoped<ICurrentUserService>(_ => new CurrentUserContextService(currentUserContext));

        _serviceProvider = builder.Build();
        _dbContext = _serviceProvider.GetRequiredService<EventCoDbContext>();
    }

    [Given(@"un événement ""(.*)"" organisé par un autre utilisateur")]
    public async Task EtantDonneUnEvenementOrganiseParUnAutreUtilisateur(string title)
    {
        var @event = Event.Create(title, null, _now.AddDays(1), null, Guid.NewGuid().ToString("N"), Guid.NewGuid(), _now);
        _dbContext.Events.Add(EventMapper.ToEntity(@event));
        await _dbContext.SaveChangesAsync();
    }

    [Given(@"je suis connecté en tant qu'administrateur")]
    public async Task EtantDonneJeSuisConnecteEnTantQuadministrateur() =>
        _currentUserContext.UserId = await AdminUserSeeder.SeedAsync(_serviceProvider);

    [Given(@"je suis connecté en tant qu'utilisateur non administrateur")]
    public async Task EtantDonneJeSuisConnecteEnTantQuutilisateurNonAdministrateur() =>
        _currentUserContext.UserId = await AdminUserSeeder.SeedAsync(_serviceProvider, isAdmin: false);

    [When(@"je consulte la liste de tous les événements")]
    public async Task QuandJeConsulteLaListeDeTousLesEvenements()
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        try
        {
            _lastResult = await dispatcher.Send(new GetAllEventsQuery(), CancellationToken.None);
        }
        catch (Exception exception)
        {
            _thrownException = exception;
        }
    }

    [Then(@"la consultation de tous les événements réussit")]
    public void AlorsLaConsultationDeTousLesEvenementsReussit() => Assert.Null(_thrownException);

    [Then(@"la consultation de tous les événements échoue avec une erreur d'autorisation administrateur")]
    public void AlorsLaConsultationDeTousLesEvenementsEchoueAvecUneErreurDautorisationAdministrateur() =>
        Assert.IsType<UserNotAdminException>(_thrownException);

    [Then(@"la liste de tous les événements contient ""(.*)"" avec (\d+) participants?")]
    public void AlorsLaListeDeTousLesEvenementsContientAvecParticipants(string title, int participantCount)
    {
        var overview = Assert.Single(_lastResult!.Events, e => e.Title == title);
        Assert.Equal(participantCount, overview.ParticipantCount);
    }

    [Then(@"la liste de tous les événements est vide")]
    public void AlorsLaListeDeTousLesEvenementsEstVide() => Assert.Empty(_lastResult!.Events);
}
