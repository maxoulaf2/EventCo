using System.Net.Http.Json;
using System.Text.RegularExpressions;
using EventCo.Api.Contracts.Auth;
using EventCo.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;

namespace EventCo.Api.Tests.Support;

[Binding]
public sealed class AuthenticatedSessionSteps(SessionContext sessionContext)
{
    private static readonly WebApplicationFactoryClientOptions ClientOptions = new() { HandleCookies = false };

    [Given(@"une session ouverte via l'API pour ""(.*)""")]
    public async Task EtantDonneUneSessionOuverteViaLapiPour(string email)
    {
        var client = Hooks.Factory.CreateClient(ClientOptions);
        await client.PostAsJsonAsync("/api/auth/request-link", new RequestMagicLinkRequest(email));

        var sentEmail = Hooks.Factory.EmailSender.SentEmails.Last(e => e.ToEmail == email.ToLowerInvariant());
        var code = ExtractCode(sentEmail.HtmlBody);

        var verifyResponse = await client.PostAsJsonAsync("/api/auth/verify", new VerifyMagicLinkRequest(email, code));
        Assert.True(verifyResponse.Headers.TryGetValues("Set-Cookie", out var cookies));
        sessionContext.Cookie = cookies!.Single(c => c.StartsWith("eventco_session=")).Split(';')[0];
    }

    // Aucun endpoint n'attribue le flag administrateur (attribution directe en base en production) :
    // la session est ouverte normalement, puis le flag est posé en base, comme le ferait le développeur.
    [Given(@"une session administrateur ouverte via l'API pour ""(.*)""")]
    public async Task EtantDonneUneSessionAdministrateurOuverteViaLapiPour(string email)
    {
        await EtantDonneUneSessionOuverteViaLapiPour(email);

        using var scope = Hooks.Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EventCoDbContext>();
        var normalizedEmail = email.ToLowerInvariant();
        await dbContext.Users
            .Where(u => u.Email == normalizedEmail)
            .ExecuteUpdateAsync(setters => setters.SetProperty(u => u.IsAdmin, true));
    }

    private static string ExtractCode(string emailHtmlBody) =>
        Regex.Match(emailHtmlBody, @">(\d{6})<").Groups[1].Value;
}
