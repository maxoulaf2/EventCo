using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Events.CreateEvent;
using EventCo.Application.Events.InviteParticipant;
using EventCo.Application.Tests.Support;
using EventCo.Application.Tests.TestDoubles;
using EventCo.Domain.Events.Exceptions;
using EventCo.Domain.Users;
using EventCo.Domain.ValueObjects;
using EventCo.Infrastructure.Persistence;
using EventCo.Infrastructure.Persistence.Repositories;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;

namespace EventCo.Application.Tests.Events.InviteParticipant;

[Binding]
public sealed class InviteParticipantSteps
{
    private readonly IServiceProvider _serviceProvider;
    private readonly EventCoDbContext _dbContext;
    private readonly DateTime _now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private Guid? _existingEventId;
    private InviteParticipantResult? _lastResult;
    private Exception? _thrownException;

    public InviteParticipantSteps()
    {
        var builder = new ApplicationTestHostBuilder();

        builder.Services.AddScoped<IEventRepository, EventRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddSingleton<IDateTimeProvider>(new FixedDateTimeProvider(_now));
        builder.Services.AddScoped<ICurrentUserService>(_ => new FixedCurrentUserService(Guid.NewGuid()));

        _serviceProvider = builder.Build();
        _dbContext = _serviceProvider.GetRequiredService<EventCoDbContext>();
    }

    [Given(@"un événement ouvert aux invitations ""(.*)"" prévu le ""(.*)"" au lieu ""(.*)""")]
    public async Task EtantDonneUnEvenementOuvertAuxInvitationsPrevuLeAuLieu(string title, string eventDate, string location)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        var createResult = await dispatcher.Send(
            new CreateEventCommand(title, null, DateTime.Parse(eventDate), location),
            CancellationToken.None);

        _existingEventId = createResult.EventId;
    }

    [Given(@"un compte existe déjà pour ""(.*)""")]
    public async Task EtantDonneUnCompteExisteDejaPour(string email)
    {
        var userRepository = _serviceProvider.GetRequiredService<IUserRepository>();
        var user = User.Create(Email.Create(email), "Ami existant", _now);
        await userRepository.AddAsync(user, CancellationToken.None);
    }

    [Given(@"""(.*)"" est déjà invité à cet événement")]
    public async Task EtantDonneEstDejaInviteACetEvenement(string email)
    {
        await Inviter(_existingEventId!.Value, email);
        Assert.Null(_thrownException);
    }

    [When(@"j'invite ""(.*)"" à cet événement")]
    public async Task QuandJinviteACetEvenement(string email) => await Inviter(_existingEventId!.Value, email);

    [When(@"j'invite ""(.*)"" à un événement inexistant")]
    public async Task QuandJinviteAUnEvenementInexistant(string email) => await Inviter(Guid.NewGuid(), email);

    private async Task Inviter(Guid eventId, string email)
    {
        var dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        _thrownException = null;

        try
        {
            _lastResult = await dispatcher.Send(new InviteParticipantCommand(eventId, email), CancellationToken.None);
        }
        catch (Exception exception)
        {
            _thrownException = exception;
        }
    }

    [Then(@"l'invitation réussit")]
    public void AlorsLinvitationReussit() => Assert.Null(_thrownException);

    [Then(@"l'invitation échoue avec une erreur de participant déjà invité")]
    public void AlorsLinvitationEchoueAvecUneErreurDeParticipantDejaInvite() =>
        Assert.IsType<ParticipantAlreadyInvitedException>(_thrownException);

    [Then(@"l'invitation échoue avec une erreur de validation")]
    public void AlorsLinvitationEchoueAvecUneErreurDeValidation() =>
        Assert.IsType<ValidationException>(_thrownException);

    [Then(@"l'invitation échoue avec une erreur d'événement introuvable")]
    public void AlorsLinvitationEchoueAvecUneErreurDevenementIntrouvable() =>
        Assert.IsType<EventNotFoundException>(_thrownException);

    [Then(@"la personne invitée a le rôle ""(.*)""")]
    public void AlorsLaPersonneInviteeALeRole(string role) => Assert.Equal(role, _lastResult!.Role);

    [Then(@"la personne invitée n'a pas encore rejoint l'événement")]
    public void AlorsLaPersonneInviteeNaPasEncoreRejointLevenement() => Assert.False(_lastResult!.HasJoined);

    [Then(@"un compte est créé pour la personne invitée ""(.*)""")]
    public void AlorsUnCompteEstCreePourLaPersonneInvitee(string email)
    {
        var user = _dbContext.Users.Single(u => u.Email == email.ToLowerInvariant());
        Assert.Equal(_lastResult!.UserId, user.Id);
    }

    [Then(@"un seul compte existe pour la personne invitée ""(.*)""")]
    public void AlorsUnSeulCompteExistePourLaPersonneInvitee(string email)
    {
        var users = _dbContext.Users.Where(u => u.Email == email.ToLowerInvariant()).ToList();
        Assert.Single(users);
    }
}
