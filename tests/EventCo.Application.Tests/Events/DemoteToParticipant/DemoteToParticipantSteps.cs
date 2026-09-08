using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Events.CreateEvent;
using EventCo.Application.Events.DemoteToParticipant;
using EventCo.Application.Events.InviteParticipant;
using EventCo.Application.Events.PromoteToOrganizer;
using EventCo.Application.Tests.Support;
using EventCo.Application.Tests.TestDoubles;
using EventCo.Domain.Events.Exceptions;
using EventCo.Domain.Users;
using EventCo.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;

namespace EventCo.Application.Tests.Events.DemoteToParticipant;

[Binding]
public sealed class DemoteToParticipantSteps
{
    private readonly IServiceProvider _serviceProvider;
    private readonly DateTime _now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private Guid? _existingEventId;
    private Guid? _creatorUserId;
    private Guid? _organizerUserId;
    private Exception? _thrownException;

    public DemoteToParticipantSteps(CurrentUserContext currentUserContext)
    {
        var builder = new ApplicationTestHostBuilder();

        builder.Services.AddScoped<IEventRepository, EventRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddSingleton<IDateTimeProvider>(new FixedDateTimeProvider(_now));
        builder.Services.AddScoped<ICurrentUserService>(_ => new CurrentUserContextService(currentUserContext));

        _serviceProvider = builder.Build();
        _creatorUserId = currentUserContext.UserId;
    }

    [Given(@"un événement ""(.*)"" avec un co-organisateur ""(.*)"", prévu le ""(.*)"" au lieu ""(.*)""")]
    public async Task EtantDonneUnEvenementAvecUnCoOrganisateur(string title, string email, string eventDate, string location)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        var createResult = await dispatcher.Send(
            new CreateEventCommand(title, null, DateTime.Parse(eventDate), location),
            CancellationToken.None);
        _existingEventId = createResult.EventId;

        var inviteResult = await dispatcher.Send(
            new InviteParticipantCommand(_existingEventId.Value, email),
            CancellationToken.None);
        _organizerUserId = inviteResult.UserId;

        await dispatcher.Send(new PromoteToOrganizerCommand(_existingEventId.Value, _organizerUserId.Value), CancellationToken.None);
    }

    [When(@"je rétrograde ce co-organisateur en participant")]
    public async Task QuandJeRetrogradeCeCoOrganisateurEnParticipant() =>
        await Retrograder(_existingEventId!.Value, _organizerUserId!.Value);

    [When(@"je rétrograde le créateur de l'événement")]
    public async Task QuandJeRetrogradeLeCreateurDeLevenement() =>
        await Retrograder(_existingEventId!.Value, _creatorUserId!.Value);

    [When(@"je rétrograde un co-organisateur sur un événement inexistant")]
    public async Task QuandJeRetrogradeUnCoOrganisateurSurUnEvenementInexistant() =>
        await Retrograder(Guid.NewGuid(), Guid.NewGuid());

    private async Task Retrograder(Guid eventId, Guid targetUserId)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        try
        {
            await dispatcher.Send(new DemoteToParticipantCommand(eventId, targetUserId), CancellationToken.None);
        }
        catch (Exception exception)
        {
            _thrownException = exception;
        }
    }

    [Then(@"la rétrogradation réussit")]
    public void AlorsLaRetrogradationReussit() => Assert.Null(_thrownException);

    [Then(@"la rétrogradation échoue avec une erreur d'autorisation")]
    public void AlorsLaRetrogradationEchoueAvecUneErreurDautorisation() =>
        Assert.IsType<UserNotEventCreatorException>(_thrownException);

    [Then(@"la rétrogradation échoue car le créateur ne peut pas être rétrogradé")]
    public void AlorsLaRetrogradationEchoueCarLeCreateurNePeutPasEtreRetrograde() =>
        Assert.IsType<EventCreatorCannotBeDemotedException>(_thrownException);

    [Then(@"la rétrogradation échoue avec une erreur d'événement introuvable")]
    public void AlorsLaRetrogradationEchoueAvecUneErreurDevenementIntrouvable() =>
        Assert.IsType<EventNotFoundException>(_thrownException);

    [Then(@"le co-organisateur rétrogradé a le rôle ""(.*)""")]
    public async Task AlorsLeCoOrganisateurRetrogradeALeRole(string role)
    {
        var eventRepository = _serviceProvider.GetRequiredService<IEventRepository>();
        var @event = await eventRepository.GetByIdAsync(_existingEventId!.Value, CancellationToken.None);
        var participant = @event!.Participants.Single(p => p.UserId == _organizerUserId!.Value);

        Assert.Equal(role, participant.Role.ToString());
    }
}
