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
    private string? _lastRawToken;

    [When(@"un lien de connexion est demandé via l'API pour ""(.*)""")]
    public async Task UnLienDeConnexionEstDemandeViaLapiPour(string email)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);
        await client.PostAsJsonAsync("/api/auth/request-link", new RequestMagicLinkRequest(email));

        var sentEmail = Hooks.Factory.EmailSender.SentEmails.Last(e => e.ToEmail == email.ToLowerInvariant());
        _lastRawToken = ExtractRawToken(sentEmail.HtmlBody);
    }

    [When(@"je valide le lien de connexion reçu via l'API")]
    public async Task JeValideLeLienDeConnexionRecuViaLapi()
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);
        _response = await client.PostAsJsonAsync("/api/auth/verify", new VerifyMagicLinkRequest(_lastRawToken!));
    }

    [When(@"j'envoie une requête POST à ""(.*)"" avec le token ""(.*)""")]
    public async Task QuandJenvoieUneRequetePostAAvecLeToken(string path, string token)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);
        _response = await client.PostAsJsonAsync(path, new VerifyMagicLinkRequest(token));
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

    [Then(@"un compte est persisté en base pour ""(.*)""")]
    public async Task AlorsUnCompteEstPersisteEnBasePour(string email)
    {
        using var scope = Hooks.Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EventCoDbContext>();
        var users = await dbContext.Users.ToListAsync();

        Assert.Contains(users, u => u.Email.Value == email);
    }

    private static string ExtractRawToken(string emailHtmlBody)
    {
        var match = Regex.Match(emailHtmlBody, @"token=([^""&]+)");
        return Uri.UnescapeDataString(match.Groups[1].Value);
    }
}
