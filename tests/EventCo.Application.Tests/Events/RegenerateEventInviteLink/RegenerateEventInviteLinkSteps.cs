using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Events.CreateEvent;
using EventCo.Application.Events.InviteParticipant;
using EventCo.Application.Events.JoinEventViaInviteLink;
using EventCo.Application.Events.PromoteToOrganizer;
using EventCo.Application.Events.RegenerateEventInviteLink;
using EventCo.Application.Tests.Support;
using EventCo.Application.Tests.TestDoubles;
using EventCo.Domain.Events.Exceptions;
using EventCo.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;

namespace EventCo.Application.Tests.Events.RegenerateEventInviteLink;

[Binding]
public sealed class RegenerateEventInviteLinkSteps
{
    private readonly IServiceProvider _serviceProvider;
    private readonly CurrentUserContext _currentUserContext;
    private readonly DateTime _now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private Guid _existingEventId;
    private Guid _organizerUserId;
    private string? _previousToken;
    private RegenerateEventInviteLinkResult? _lastResult;
    private Exception? _thrownException;

    public RegenerateEventInviteLinkSteps(CurrentUserContext currentUserContext)
    {
        _currentUserContext = currentUserContext;

        var builder = new ApplicationTestHostBuilder();

        builder.Services.AddScoped<IEventRepository, EventRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddSingleton<IDateTimeProvider>(new FixedDateTimeProvider(_now));
        builder.Services.AddScoped<ICurrentUserService>(_ => new CurrentUserContextService(currentUserContext));

        _serviceProvider = builder.Build();
    }

    [Given(@"un événement à régénérer ""(.*)"" prévu le ""(.*)"" au lieu ""(.*)""")]
    public async Task EtantDonneUnEvenementARegenererPrevuLeAuLieu(string title, string eventDate, string location)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        var createResult = await dispatcher.Send(
            new CreateEventCommand(title, null, DateTime.Parse(eventDate), location),
            CancellationToken.None);

        _existingEventId = createResult.EventId;

        var eventRepository = _serviceProvider.GetRequiredService<IEventRepository>();
        var @event = await eventRepository.GetByIdAsync(_existingEventId, CancellationToken.None);
        _previousToken = @event!.InviteLinkToken;
    }

    [Given(@"un co-organisateur ""(.*)"" pour cet événement")]
    public async Task EtantDonneUnCoOrganisateurPourCetEvenement(string email)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        var inviteResult = await dispatcher.Send(new InviteParticipantCommand(_existingEventId, email), CancellationToken.None);
        _organizerUserId = inviteResult.UserId;

        await dispatcher.Send(new PromoteToOrganizerCommand(_existingEventId, _organizerUserId), CancellationToken.None);
    }

    [When(@"je régénère le lien d'invitation de cet événement")]
    public async Task QuandJeRegenereLeLienDinvitationDeCetEvenement() =>
        await Regenerer(_existingEventId);

    [When(@"je régénère le lien d'invitation de cet événement en tant que co-organisateur")]
    public async Task QuandJeRegenereLeLienDinvitationDeCetEvenementEnTantQueCoOrganisateur()
    {
        _currentUserContext.UserId = _organizerUserId;
        await Regenerer(_existingEventId);
    }

    [When(@"je régénère le lien d'invitation d'un événement inexistant")]
    public async Task QuandJeRegenereLeLienDinvitationDunEvenementInexistant() =>
        await Regenerer(Guid.NewGuid());

    private async Task Regenerer(Guid eventId)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        _thrownException = null;

        try
        {
            _lastResult = await dispatcher.Send(new RegenerateEventInviteLinkCommand(eventId), CancellationToken.None);
        }
        catch (Exception exception)
        {
            _thrownException = exception;
        }
    }

    [Then(@"la régénération réussit")]
    public void AlorsLaRegenerationReussit() => Assert.Null(_thrownException);

    [Then(@"la régénération échoue avec une erreur d'autorisation")]
    public void AlorsLaRegenerationEchoueAvecUneErreurDautorisation() =>
        Assert.IsType<UserNotEventOrganizerException>(_thrownException);

    [Then(@"la régénération échoue avec une erreur d'événement introuvable")]
    public void AlorsLaRegenerationEchoueAvecUneErreurDevenementIntrouvable() =>
        Assert.IsType<EventNotFoundException>(_thrownException);

    [Then(@"le nouveau lien diffère de l'ancien")]
    public void AlorsLeNouveauLienDiffereDeLancien() =>
        Assert.NotEqual(_previousToken, _lastResult!.InviteLinkToken);

    [Then(@"l'ancien lien ne permet plus de rejoindre l'événement")]
    public async Task AlorsLancienLienNePermetPlusDeRejoindreLevenement()
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();

        await Assert.ThrowsAsync<InviteLinkNotFoundException>(
            () => dispatcher.Send(new JoinEventViaInviteLinkCommand(_previousToken!), CancellationToken.None));
    }
}
