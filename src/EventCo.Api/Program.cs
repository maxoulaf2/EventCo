using EventCo.Api.Auth;
using EventCo.Api.ExceptionHandling;
using EventCo.Application;
using EventCo.Application.Common.Interfaces;
using EventCo.Infrastructure;
using EventCo.Infrastructure.Realtime;
using Microsoft.AspNetCore.Authentication;

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
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy.WithOrigins(builder.Configuration["Frontend:BaseUrl"] ?? "http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
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

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseCors(FrontendCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<EventHub>("/hubs/events");

app.Run();

public partial class Program;
