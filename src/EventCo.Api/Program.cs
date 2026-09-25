using EventCo.Api.Auth;
using EventCo.Api.Contracts.Versioning;
using EventCo.Api.ExceptionHandling;
using EventCo.Api.Versioning;
using EventCo.Application;
using EventCo.Application.Common.Interfaces;
using EventCo.Infrastructure;
using EventCo.Infrastructure.Realtime;
using EventCo.Infrastructure.Storage;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

const string FrontendCorsPolicy = "Frontend";
// Valeur committée dans appsettings.json (base, chargée dans tous les environnements) pour permettre
// `dotnet run`/docker-compose en dev sans configuration supplémentaire — jamais destinée à signer des
// sessions en production. Cf. garde plus bas.
const string DevSessionSecretPlaceholder = "dev-secret-a-remplacer-en-production-via-variable-environnement";

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, HttpContextCurrentUserService>();
builder.Services
    .AddAuthentication(SessionAuthenticationDefaults.AuthenticationScheme)
    .AddScheme<AuthenticationSchemeOptions, SessionAuthenticationHandler>(
        SessionAuthenticationDefaults.AuthenticationScheme, _ => { });
// TODO mieux gérer les CORS
builder.Services.AddSingleton(sp => AppVersion.FromConfiguration(sp.GetRequiredService<IConfiguration>()));
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy.WithOrigins(builder.Configuration["Frontend:BaseUrl"] ?? "http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithExposedHeaders(AppVersion.HeaderName);
    });
});

var app = builder.Build();

// Échoue au démarrage plutôt que de signer silencieusement des sessions (cf. SessionTokenService,
// HMAC) avec le secret de dev committé — sans cette garde, oublier de définir Session:Secret en
// dehors de Development permettrait à quiconque connaît ce secret (public, dans ce repo) de forger
// un token de session valide pour n'importe quel utilisateur.
if (!app.Environment.IsDevelopment())
{
    var sessionSecret = app.Configuration["Session:Secret"];
    if (string.IsNullOrWhiteSpace(sessionSecret) || sessionSecret == DevSessionSecretPlaceholder)
        throw new InvalidOperationException(
            "Session:Secret doit être défini via une variable d'environnement (ou un secret manager) en dehors de Development.");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var appVersion = app.Services.GetRequiredService<AppVersion>();
app.Logger.LogInformation("Version de l'application : {AppVersion}", appVersion.Value);

// Posé sur toutes les réponses (erreurs comprises, d'où sa place avant UseExceptionHandler) : chaque appel
// du frontend lui permet de détecter qu'il ne tourne plus dans la même version que l'API
// (cf. client/src/shared/lib/appVersion.ts).
app.Use((context, next) =>
{
    context.Response.OnStarting(() =>
    {
        context.Response.Headers[AppVersion.HeaderName] = appVersion.Value;
        return Task.CompletedTask;
    });
    return next(context);
});

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseCors(FrontendCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

// Health check pour Render (et autres PaaS) : doit répondre avant toute dépendance à la base/l'auth.
app.MapGet("/health", () => Results.Ok());

// Anonyme et jamais mis en cache : interrogé par le frontend au retour sur l'onglet et à la reconnexion
// SignalR, pour détecter un déploiement survenu pendant qu'il était ouvert.
app.MapGet("/api/version", (HttpContext context) =>
{
    context.Response.Headers.CacheControl = "no-store";
    return Results.Ok(new AppVersionResponse(appVersion.Value));
});

// Fallback de stockage sur disque (aucun bucket S3 configuré, cf. LocalFileStorage) : les fichiers sont
// servis par l'API elle-même. Avec un bucket S3, ils sont lus directement dessus (URL publique).
if (app.Services.GetRequiredService<IFileStorage>() is LocalFileStorage localFileStorage)
{
    if (!app.Environment.IsDevelopment())
        app.Logger.LogWarning(
            "Aucun bucket S3 configuré (Storage:S3:ServiceUrl) : fichiers stockés sur le disque local ({RootDirectory}), perdus à chaque redéploiement.",
            localFileStorage.RootDirectory);

    Directory.CreateDirectory(localFileStorage.RootDirectory);
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(localFileStorage.RootDirectory),
        RequestPath = localFileStorage.RequestPath,
    });
}

app.MapControllers();
app.MapHub<EventHub>("/hubs/events");

// Sert le frontend buildé (wwwroot, cf. Dockerfile) en production : same-origin avec l'API, cf. note
// dans AuthController sur SameSite=Lax. wwwroot est absent en dev (frontend servi par Vite), donc
// ce bloc ne s'active qu'en présence du build statique.
if (Directory.Exists(Path.Combine(app.Environment.ContentRootPath, "wwwroot")))
{
    // index.html et le service worker référencent les bundles hashés du build courant : toujours revalidés,
    // pour qu'un rechargement après détection d'une nouvelle version (cf. AppVersion) ne ressorte pas
    // l'ancien index.html du cache HTTP du navigateur. Les bundles hashés gardent le cache par défaut.
    var frontendStaticFileOptions = new StaticFileOptions
    {
        OnPrepareResponse = context =>
        {
            if (context.File.Name is "index.html" or "sw.js")
                context.Context.Response.Headers.CacheControl = "no-cache";
        },
    };

    app.UseDefaultFiles();
    app.UseStaticFiles(frontendStaticFileOptions);
    app.MapFallbackToFile("index.html", frontendStaticFileOptions);
}

app.Run();

public partial class Program;
