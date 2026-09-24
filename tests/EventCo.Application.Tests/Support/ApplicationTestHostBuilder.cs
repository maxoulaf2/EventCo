using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Options;
using EventCo.Application.Tests.TestDoubles;
using EventCo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EventCo.Application.Tests.Support;

public sealed class ApplicationTestHostBuilder
{
    private readonly ServiceCollection _services = new();

    public ApplicationTestHostBuilder()
    {
        _services.AddApplication();
        _services.AddDbContext<EventCoDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        _services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Dépendances de InviteParticipantCommandHandler, utilisé comme fixture par de nombreux autres
        // Steps (co-organisateur, participant...) sans qu'ils aient besoin de connaître ces détails :
        // valeurs par défaut ici, surchargeables (AddSingleton après Build()) par les Steps qui testent
        // spécifiquement l'envoi d'email (ex: InviteParticipantSteps, RequestLoginCodeSteps).
        _services.AddSingleton<IEmailSender>(new RecordingEmailSender());
        _services.AddSingleton(Options.Create(new InvitationOptions()));
        _services.AddSingleton(Options.Create(new FrontendOptions { BaseUrl = "http://localhost:5173" }));

        // Dépendance de toute query/command qui expose la photo de profil d'un utilisateur (utilisateur
        // courant, participants d'un événement...) : surchargeable par les Steps qui observent le stockage.
        _services.AddSingleton<IFileStorage>(new InMemoryFileStorage());
    }

    public IServiceCollection Services => _services;

    public IServiceProvider Build() => _services.BuildServiceProvider();
}
