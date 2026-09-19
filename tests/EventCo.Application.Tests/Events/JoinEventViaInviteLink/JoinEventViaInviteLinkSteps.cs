using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Events.CreateEvent;
using EventCo.Application.Events.JoinEventViaInviteLink;
using EventCo.Application.Tests.Support;
using EventCo.Application.Tests.TestDoubles;
using EventCo.Domain.Events.Exceptions;
using EventCo.Infrastructure.Persistence;
using EventCo.Infrastructure.Persistence.Entities;
using EventCo.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;

namespace EventCo.Application.Tests.Events.JoinEventViaInviteLink;

[Binding]
public sealed class JoinEventViaInviteLinkSteps
{
    private readonly IServiceProvider _serviceProvider;
    private readonly EventCoDbContext _dbContext;
    private readonly CurrentUserContext _currentUserContext;
    private readonly DateTime _now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private Guid _existingEventId;
    private JoinEventViaInviteLinkResult? _lastResult;
    private Exception? _thrownException;

    public JoinEventViaInviteLinkSteps(CurrentUserContext currentUserContext)
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

    [Given(@"un événement à rejoindre ""(.*)"" prévu le ""(.*)"" au lieu ""(.*)""")]
    public async Task EtantDonneUnEvenementARejoindrePrevuLeAuLieu(string title, string eventDate, string location)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        var createResult = await dispatcher.Send(
            new CreateEventCommand(title, null, DateTime.Parse(eventDate), location),
            CancellationToken.None);

        _existingEventId = createResult.EventId;
    }

    [Given(@"je rejoins l'événement via son lien d'invitation")]
    [When(@"je rejoins l'événement via son lien d'invitation")]
    public async Task QuandJeRejoinsLevenementViaSonLienDinvitation()
    {
        var eventRepository = _serviceProvider.GetRequiredService<IEventRepository>();
        var @event = await eventRepository.GetByIdAsync(_existingEventId, CancellationToken.None);

        await Rejoindre(@event!.InviteLinkToken);
    }

    [When(@"je rejoins un événement via un lien d'invitation inexistant")]
    public async Task QuandJeRejoinsUnEvenementViaUnLienDinvitationInexistant() =>
        await Rejoindre("token-qui-n-existe-pas");

    private async Task Rejoindre(string token)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        _thrownException = null;

        try
        {
            _lastResult = await dispatcher.Send(new JoinEventViaInviteLinkCommand(token), CancellationToken.None);
        }
        catch (Exception exception)
        {
            _thrownException = exception;
        }
    }

    [Then(@"je rejoins l'événement avec succès")]
    public void AlorsJeRejoinsLevenementAvecSucces() => Assert.Null(_thrownException);

    [Then(@"la tentative échoue avec une erreur de lien introuvable")]
    public void AlorsLaTentativeEchoueAvecUneErreurDeLienIntrouvable() =>
        Assert.IsType<InviteLinkNotFoundException>(_thrownException);

    [Then(@"j'ai le rôle ""(.*)"" dans cet événement")]
    public void AlorsJaiLeRoleDansCetEvenement(string role)
    {
        var participant = _dbContext.Set<EventParticipantEntity>().Single(p =>
            p.EventId == _lastResult!.EventId && p.UserId == _currentUserContext.UserId);

        Assert.Equal(role, participant.Role.ToString());
    }
}
