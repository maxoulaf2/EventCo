using EventCo.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace EventCo.Infrastructure.Emailing;

// Provisoire : journalise l'email au lieu de l'envoyer, en attendant un vrai fournisseur (suivi-todo.md, lot 1).
internal sealed class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Email (non envoyé, fournisseur non configuré) à {ToEmail} — Sujet : {Subject}\n{Body}",
            toEmail,
            subject,
            htmlBody);

        return Task.CompletedTask;
    }
}
