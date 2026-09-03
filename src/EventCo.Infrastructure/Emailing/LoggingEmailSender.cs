using EventCo.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace EventCo.Infrastructure.Emailing;

// Fallback quand aucun serveur SMTP n'est configuré (Email:Smtp:Host vide) : journalise l'email
// au lieu de l'envoyer, utile en dev tant qu'un compte Mailtrap/Resend/SendGrid n'est pas renseigné.
internal sealed class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Email (non envoyé, aucun serveur SMTP configuré) à {ToEmail} — Sujet : {Subject}\n{Body}",
            toEmail,
            subject,
            htmlBody);

        return Task.CompletedTask;
    }
}
