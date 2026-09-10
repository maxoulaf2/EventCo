using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Events.CreateEvent;
using EventCo.Application.Events.GetEventById;
using EventCo.Application.Events.InviteParticipant;
using EventCo.Application.Tests.Support;
using EventCo.Application.Tests.TestDoubles;
using EventCo.Domain.Events.Exceptions;
using EventCo.Domain.Users;
using EventCo.Domain.ValueObjects;
using EventCo.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;

namespace EventCo.Application.Tests.Events.GetEventById;

[Binding]
public sealed class GetEventByIdSteps
{
    private readonly IServiceProvider _serviceProvider;
    private readonly CurrentUserContext _currentUserContext;
    private readonly DateTime _now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private Guid? _existingEventId;
    private GetEventByIdResult? _lastResult;
    private Exception? _thrownException;

    public GetEventByIdSteps(CurrentUserContext currentUserContext)
    {
        _currentUserContext = currentUserContext;

        var builder = new ApplicationTestHostBuilder();

        builder.Services.AddScoped<IEventRepository, EventRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddSingleton<IDateTimeProvider>(new FixedDateTimeProvider(_now));
        builder.Services.AddScoped<ICurrentUserService>(_ => new CurrentUserContextService(currentUserContext));

        _serviceProvider = builder.Build();
    }

    [Given(@"""(.*)"" est invité à cet événement")]
    public async Task EtantDonneEstInviteACetEvenement(string email)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        await dispatcher.Send(new InviteParticipantCommand(_existingEventId!.Value, email), CancellationToken.None);
    }

    [Given(@"un événement ""(.*)"" prévu le ""(.*)"" au lieu ""(.*)""")]
    public async Task EtantDonneUnEvenementPrevuLeAuLieu(string title, string eventDate, string location)
    {
        var userRepository = _serviceProvider.GetRequiredService<IUserRepository>();
        var organizer = User.Create(Email.Create("organisateur@example.com"), "Organisateur", _now);
        await userRepository.ApplyAsync(organizer, CancellationToken.None);

        // Écriture repository directe, hors ICommandDispatcher : à committer explicitement (cf. même remarque
        // dans InviteParticipantSteps).
        await _serviceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync(CancellationToken.None);
        _currentUserContext.UserId = organizer.Id;

        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        var createResult = await dispatcher.Send(
            new CreateEventCommand(title, null, DateTime.Parse(eventDate), location),
            CancellationToken.None);

        _existingEventId = createResult.EventId;
    }

    [When(@"je consulte cet événement")]
    public async Task QuandJeConsulteCetEvenement()
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        try
        {
            _lastResult = await dispatcher.Send(new GetEventByIdQuery(_existingEventId!.Value), CancellationToken.None);
        }
        catch (Exception exception)
        {
            _thrownException = exception;
        }
    }

    [When(@"je consulte un événement inexistant")]
    public async Task QuandJeConsulteUnEvenementInexistant()
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        try
        {
            _lastResult = await dispatcher.Send(new GetEventByIdQuery(Guid.NewGuid()), CancellationToken.None);
        }
        catch (Exception exception)
        {
            _thrownException = exception;
        }
    }

    [Then(@"la consultation réussit")]
    public void AlorsLaConsultationReussit() => Assert.Null(_thrownException);

    [Then(@"la consultation échoue avec une erreur d'événement introuvable")]
    public void AlorsLaConsultationEchoueAvecUneErreurDevenementIntrouvable() =>
        Assert.IsType<EventNotFoundException>(_thrownException);

    [Then(@"la consultation échoue avec une erreur d'autorisation")]
    public void AlorsLaConsultationEchoueAvecUneErreurDautorisation() =>
        Assert.IsType<UserNotEventParticipantException>(_thrownException);

    [Then(@"l'événement consulté a pour titre ""(.*)""")]
    public void AlorsLevenementConsulteAPourTitre(string title) => Assert.Equal(title, _lastResult!.Title);

    [Then(@"l'événement consulté a (\d+) participants?")]
    public void AlorsLevenementConsulteAParticipants(int count) => Assert.Equal(count, _lastResult!.Participants.Count);

    [Then(@"l'événement consulté a un participant ""(.*)"" avec le rôle ""(.*)"" n'ayant pas encore rejoint")]
    public void AlorsLevenementConsulteAUnParticipantAvecLeRoleNayantPasEncoreRejoint(string email, string role)
    {
        var participant = Assert.Single(_lastResult!.Participants, p => p.Email == email.ToLowerInvariant());
        Assert.Equal(role, participant.Role);
        Assert.False(participant.HasJoined);
    }
}
