using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EventCo.Infrastructure.Emailing;

// SMTP générique plutôt qu'un SDK par fournisseur (Resend/SendGrid ont tous deux un relais SMTP,
// tout comme Mailtrap en dev) : un seul sender, sélectionné par la config (cf. DependencyInjection).
internal sealed class SmtpEmailSender(IOptions<EmailOptions> options) : IEmailSender
{
    public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        var emailOptions = options.Value;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(emailOptions.FromName, emailOptions.FromAddress));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        var smtp = emailOptions.Smtp;

        await client.ConnectAsync(smtp.Host, smtp.Port, SecureSocketOptions.Auto, cancellationToken);

        if (!string.IsNullOrEmpty(smtp.Username))
        {
            await client.AuthenticateAsync(smtp.Username, smtp.Password, cancellationToken);
        }

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
