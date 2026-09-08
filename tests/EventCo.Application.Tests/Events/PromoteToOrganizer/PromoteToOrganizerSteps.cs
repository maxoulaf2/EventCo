using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Events.CreateEvent;
using EventCo.Application.Events.InviteParticipant;
using EventCo.Application.Events.PromoteToOrganizer;
using EventCo.Application.Tests.Support;
using EventCo.Application.Tests.TestDoubles;
using EventCo.Domain.Events.Exceptions;
using EventCo.Domain.Users;
using EventCo.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;

namespace EventCo.Application.Tests.Events.PromoteToOrganizer;

[Binding]
public sealed class PromoteToOrganizerSteps
{
    private readonly IServiceProvider _serviceProvider;
    private readonly DateTime _now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private Guid? _existingEventId;
    private Guid? _participantUserId;
    private Exception? _thrownException;

    public PromoteToOrganizerSteps(CurrentUserContext currentUserContext)
    {
        var builder = new ApplicationTestHostBuilder();

        builder.Services.AddScoped<IEventRepository, EventRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddSingleton<IDateTimeProvider>(new FixedDateTimeProvider(_now));
        builder.Services.AddScoped<ICurrentUserService>(_ => new CurrentUserContextService(currentUserContext));

        _serviceProvider = builder.Build();
    }

    [Given(@"un événement ""(.*)"" avec un participant invité ""(.*)"", prévu le ""(.*)"" au lieu ""(.*)""")]
    public async Task EtantDonneUnEvenementAvecUnParticipantInvite(string title, string email, string eventDate, string location)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        var createResult = await dispatcher.Send(
            new CreateEventCommand(title, null, DateTime.Parse(eventDate), location),
            CancellationToken.None);
        _existingEventId = createResult.EventId;

        var inviteResult = await dispatcher.Send(
            new InviteParticipantCommand(_existingEventId.Value, email),
            CancellationToken.None);
        _participantUserId = inviteResult.UserId;
    }

    [When(@"je promeus ce participant en co-organisateur")]
    public async Task QuandJePromeusCeParticipantEnCoOrganisateur() =>
        await Promouvoir(_existingEventId!.Value, _participantUserId!.Value);

    [When(@"je promeus un utilisateur qui ne participe pas à l'événement")]
    public async Task QuandJePromeusUnUtilisateurQuiNeParticipePasALevenement() =>
        await Promouvoir(_existingEventId!.Value, Guid.NewGuid());

    [When(@"je promeus un participant sur un événement inexistant")]
    public async Task QuandJePromeusUnParticipantSurUnEvenementInexistant() =>
        await Promouvoir(Guid.NewGuid(), Guid.NewGuid());

    private async Task Promouvoir(Guid eventId, Guid targetUserId)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        try
        {
            await dispatcher.Send(new PromoteToOrganizerCommand(eventId, targetUserId), CancellationToken.None);
        }
        catch (Exception exception)
        {
            _thrownException = exception;
        }
    }

    [Then(@"la promotion réussit")]
    public void AlorsLaPromotionReussit() => Assert.Null(_thrownException);

    [Then(@"la promotion échoue avec une erreur d'autorisation")]
    public void AlorsLaPromotionEchoueAvecUneErreurDautorisation() =>
        Assert.IsType<UserNotEventCreatorException>(_thrownException);

    [Then(@"la promotion échoue avec une erreur de participant introuvable")]
    public void AlorsLaPromotionEchoueAvecUneErreurDeParticipantIntrouvable() =>
        Assert.IsType<ParticipantNotFoundException>(_thrownException);

    [Then(@"la promotion échoue avec une erreur d'événement introuvable")]
    public void AlorsLaPromotionEchoueAvecUneErreurDevenementIntrouvable() =>
        Assert.IsType<EventNotFoundException>(_thrownException);

    [Then(@"le participant a le rôle ""(.*)""")]
    public async Task AlorsLeParticipantALeRole(string role)
    {
        var eventRepository = _serviceProvider.GetRequiredService<IEventRepository>();
        var @event = await eventRepository.GetByIdAsync(_existingEventId!.Value, CancellationToken.None);
        var participant = @event!.Participants.Single(p => p.UserId == _participantUserId!.Value);

        Assert.Equal(role, participant.Role.ToString());
    }
}
