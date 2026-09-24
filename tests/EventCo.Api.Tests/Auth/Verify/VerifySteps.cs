using System.Net.Http.Json;
using System.Text.RegularExpressions;
using EventCo.Api.Contracts.Auth;
using EventCo.Api.Tests.Support;
using EventCo.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;

namespace EventCo.Api.Tests.Auth.Verify;

[Binding]
public sealed class VerifySteps
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };

    private HttpResponseMessage? _response;
    private string? _lastEmail;
    private string? _lastCode;

    [When(@"un code de connexion est demandé via l'API pour ""(.*)""")]
    public async Task UnCodeDeConnexionEstDemandeViaLapiPour(string email)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);
        await client.PostAsJsonAsync("/api/auth/request-link", new RequestMagicLinkRequest(email));

        var sentEmail = Hooks.Factory.EmailSender.SentEmails.Last(e => e.ToEmail == email.ToLowerInvariant());
        _lastEmail = email;
        _lastCode = ExtractCode(sentEmail.HtmlBody);
    }

    [When(@"je valide le code de connexion reçu via l'API")]
    public async Task JeValideLeCodeDeConnexionRecuViaLapi()
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);
        _response = await client.PostAsJsonAsync("/api/auth/verify", new VerifyMagicLinkRequest(_lastEmail!, _lastCode!));
    }

    [When(@"je valide un code erroné via l'API")]
    public async Task JeValideUnCodeErroneViaLapi()
    {
        var wrongCode = _lastCode == "000000" ? "000001" : "000000";
        var client = Hooks.Factory.CreateClient(ClientOptions);
        _response = await client.PostAsJsonAsync("/api/auth/verify", new VerifyMagicLinkRequest(_lastEmail!, wrongCode));
    }

    [When(@"je valide via l'API le code ""(.*)"" pour l'email ""(.*)""")]
    public async Task JeValideViaLapiLeCodePourLemail(string code, string email)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);
        _response = await client.PostAsJsonAsync("/api/auth/verify", new VerifyMagicLinkRequest(email, code));
    }

    [Then(@"la réponse de vérification a le statut (\d+)")]
    public void AlorsLaReponseDeVerificationALeStatut(int expectedStatusCode)
    {
        Assert.Equal(expectedStatusCode, (int)_response!.StatusCode);
    }

    [Then(@"un cookie de session httpOnly est présent dans la réponse")]
    public void AlorsUnCookieDeSessionHttpOnlyEstPresentDansLaReponse()
    {
        Assert.True(_response!.Headers.TryGetValues("Set-Cookie", out var cookies));
        Assert.Contains(cookies!, c => c.Contains("eventco_session=") && c.Contains("HttpOnly", StringComparison.OrdinalIgnoreCase));
    }

    [Then(@"aucun cookie de session n'est présent dans la réponse")]
    public void AlorsAucunCookieDeSessionNestPresentDansLaReponse()
    {
        Assert.False(_response!.Headers.TryGetValues("Set-Cookie", out _));
    }

    [Then(@"un compte est persisté en base pour ""(.*)""")]
    public async Task AlorsUnCompteEstPersisteEnBasePour(string email)
    {
        using var scope = Hooks.Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EventCoDbContext>();
        var users = await dbContext.Users.ToListAsync();

        Assert.Contains(users, u => u.Email == email);
    }

    [Then(@"(\d+) essai erroné est persisté en base pour ""(.*)""")]
    public async Task AlorsEssaiErroneEstPersisteEnBasePour(int expectedFailedAttempts, string email)
    {
        using var scope = Hooks.Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EventCoDbContext>();
        var token = await dbContext.MagicLinkTokens.SingleAsync(t => t.Email == email);

        Assert.Equal(expectedFailedAttempts, token.FailedAttempts);
    }

    private static string ExtractCode(string emailHtmlBody) =>
        Regex.Match(emailHtmlBody, @">(\d{6})<").Groups[1].Value;
}
