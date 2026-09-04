using System.Net.Http.Json;
using System.Text.RegularExpressions;
using EventCo.Api.Contracts.Auth;
using Microsoft.AspNetCore.Mvc.Testing;
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
        var rawToken = ExtractRawToken(sentEmail.HtmlBody);

        var verifyResponse = await client.PostAsJsonAsync("/api/auth/verify", new VerifyMagicLinkRequest(rawToken));
        Assert.True(verifyResponse.Headers.TryGetValues("Set-Cookie", out var cookies));
        sessionContext.Cookie = cookies!.Single(c => c.StartsWith("eventco_session=")).Split(';')[0];
    }

    private static string ExtractRawToken(string emailHtmlBody)
    {
        var match = Regex.Match(emailHtmlBody, @"token=([^""&]+)");
        return Uri.UnescapeDataString(match.Groups[1].Value);
    }
}
