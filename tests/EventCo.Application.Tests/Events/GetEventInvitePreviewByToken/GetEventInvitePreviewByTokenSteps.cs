using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Events.CreateEvent;
using EventCo.Application.Events.GetEventInvitePreviewByToken;
using EventCo.Application.Tests.Support;
using EventCo.Application.Tests.TestDoubles;
using EventCo.Domain.Events.Exceptions;
using EventCo.Domain.Users;
using EventCo.Domain.ValueObjects;
using EventCo.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;

namespace EventCo.Application.Tests.Events.GetEventInvitePreviewByToken;

[Binding]
public sealed class GetEventInvitePreviewByTokenSteps
{
    private readonly IServiceProvider _serviceProvider;
    private readonly CurrentUserContext _currentUserContext;
    private readonly DateTime _now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private Guid _existingEventId;
    private EventInvitePreviewResult? _lastResult;
    private Exception? _thrownException;

    public GetEventInvitePreviewByTokenSteps(CurrentUserContext currentUserContext)
    {
        _currentUserContext = currentUserContext;

        var builder = new ApplicationTestHostBuilder();

        builder.Services.AddScoped<IEventRepository, EventRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddSingleton<IDateTimeProvider>(new FixedDateTimeProvider(_now));
        builder.Services.AddScoped<ICurrentUserService>(_ => new CurrentUserContextService(currentUserContext));

        _serviceProvider = builder.Build();
    }

    [Given(@"un événement avec aperçu ""(.*)"" prévu le ""(.*)"" au lieu ""(.*)""")]
    public async Task EtantDonneUnEvenementAvecApercuPrevuLeAuLieu(string title, string eventDate, string location)
    {
        // La consultation d'aperçu résout le nom d'affichage du créateur via IUserRepository :
        // il lui faut un vrai compte (pas seulement un userId de fixture), comme dans GetEventByIdSteps.
        var userRepository = _serviceProvider.GetRequiredService<IUserRepository>();
        var creator = User.Create(Email.From("organisateur@example.com"), "Organisateur", _now);
        await userRepository.ApplyAsync(creator, CancellationToken.None);
        await _serviceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync(CancellationToken.None);
        _currentUserContext.UserId = creator.Id;

        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        var createResult = await dispatcher.Send(
            new CreateEventCommand(title, null, DateTime.Parse(eventDate), location),
            CancellationToken.None);

        _existingEventId = createResult.EventId;
    }

    [When(@"je consulte l'aperçu de cet événement via son lien d'invitation")]
    public async Task QuandJeConsulteLapercuDeCetEvenementViaSonLienDinvitation()
    {
        var eventRepository = _serviceProvider.GetRequiredService<IEventRepository>();
        var @event = await eventRepository.GetByIdAsync(_existingEventId, CancellationToken.None);

        await ConsulterApercu(@event!.InviteLinkToken);
    }

    [When(@"je consulte l'aperçu d'un événement via un lien d'invitation inexistant")]
    public async Task QuandJeConsulteLapercuDunEvenementViaUnLienDinvitationInexistant() =>
        await ConsulterApercu("token-qui-n-existe-pas");

    private async Task ConsulterApercu(string token)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        _thrownException = null;

        try
        {
            _lastResult = await dispatcher.Send(new GetEventInvitePreviewByTokenQuery(token), CancellationToken.None);
        }
        catch (Exception exception)
        {
            _thrownException = exception;
        }
    }

    [Then(@"la consultation de l'aperçu réussit")]
    public void AlorsLaConsultationDeLapercuReussit() => Assert.Null(_thrownException);

    [Then(@"la consultation de l'aperçu échoue avec une erreur de lien introuvable")]
    public void AlorsLaConsultationDeLapercuEchoueAvecUneErreurDeLienIntrouvable() =>
        Assert.IsType<InviteLinkNotFoundException>(_thrownException);

    [Then(@"l'aperçu indique le titre ""(.*)""")]
    public void AlorsLapercuIndiqueLeTitre(string title) => Assert.Equal(title, _lastResult!.Title);

    [Then(@"l'aperçu indique le lieu ""(.*)""")]
    public void AlorsLapercuIndiqueLeLieu(string location) => Assert.Equal(location, _lastResult!.Location);
}
