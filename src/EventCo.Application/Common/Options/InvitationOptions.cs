namespace EventCo.Application.Common.Options;

public sealed class InvitationOptions
{
    public const string SectionName = "Invitation";

    // Anti-spam : au-delà de MaxEmailsPerWindow emails d'invitation envoyés à la même personne (tous
    // événements confondus) sur RateLimitWindowMinutes, les invitations suivantes sont rejetées
    // (cf. TooManyInvitationEmailsException).
    public int MaxEmailsPerWindow { get; init; } = 5;

    public int RateLimitWindowMinutes { get; init; } = 60;
}
