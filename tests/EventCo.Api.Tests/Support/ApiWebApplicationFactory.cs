using EventCo.Application.Common.Interfaces;
using EventCo.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;

namespace EventCo.Api.Tests.Support;

public sealed class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();

    // L'envoi d'email réel n'est pas disponible en test (LoggingEmailSender se contente de logguer) :
    // on substitue ce port externe par un double observable, comme pour Application.Tests.
    public RecordingEmailSender EmailSender { get; } = new();

    // Version simulant un build déployé (SHA git injecté par le Dockerfile), cf. AppVersion.
    public const string AppVersion = "api-tests-version";

    // Aucun bucket S3 en test : c'est le fallback LocalFileStorage (réel, sur disque) qui est exercé, dans
    // un dossier temporaire propre à ce run.
    private readonly string _storageRootPath = Path.Combine(Path.GetTempPath(), $"eventco-api-tests-{Guid.NewGuid():N}");

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<EventCoDbContext>().Database.MigrateAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Production);

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = _postgres.GetConnectionString(),
                ["Session:Secret"] = "api-tests-secret-simulant-un-secret-de-production",
                ["Storage:Local:RootPath"] = _storageRootPath,
                ["App:Version"] = ApiWebApplicationFactory.AppVersion,
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<IEmailSender>(EmailSender);
        });
    }

    public async Task StopAsync()
    {
        await _postgres.DisposeAsync();
        await DisposeAsync();

        if (Directory.Exists(_storageRootPath))
            Directory.Delete(_storageRootPath, recursive: true);
    }
}
