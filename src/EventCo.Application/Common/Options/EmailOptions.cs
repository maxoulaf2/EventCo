namespace EventCo.Application.Common.Options;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public string FromAddress { get; init; } = "no-reply@eventco.local";

    public string FromName { get; init; } = "EventCo";

    public SmtpOptions Smtp { get; init; } = new();
}
