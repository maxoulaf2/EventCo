using System.Net;
using EventCo.Application.Common.Interfaces;
using EventCo.Application.Common.Messaging;
using EventCo.Application.Common.Options;
using EventCo.Domain.Events.Exceptions;
using EventCo.Domain.Users;
using EventCo.Domain.ValueObjects;
using Microsoft.Extensions.Options;

namespace EventCo.Application.Events.InviteParticipant;

public sealed class InviteParticipantCommandHandler(
    ICurrentUserService currentUserService,
    IEventRepository eventRepository,
    IUserRepository userRepository,
    IEmailSender emailSender,
    IFileStorage fileStorage,
    IDateTimeProvider dateTimeProvider,
    IOptions<InvitationOptions> invitationOptions,
    IOptions<FrontendOptions> frontendOptions) : ICommandHandler<InviteParticipantCommand, InviteParticipantResult>
{
    public async Task<InviteParticipantResult> Handle(InviteParticipantCommand request, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(request.EventId, cancellationToken)
            ?? throw new EventNotFoundException(request.EventId);

        var email = Email.From(request.Email);
        var now = dateTimeProvider.UtcNow;

        var user = await userRepository.GetByEmailAsync(email, cancellationToken);
        if (user is null)
        {
            user = User.Create(email, DisplayNameFromEmail(email.Value), now);
            await userRepository.ApplyAsync(user, cancellationToken);
        }

        var invitationOptionsValue = invitationOptions.Value;
        var windowStart = now.AddMinutes(-invitationOptionsValue.RateLimitWindowMinutes);
        var recentInvitationCount = await eventRepository.CountInvitationsForUserSinceAsync(user.Id, windowStart, cancellationToken);
        if (recentInvitationCount >= invitationOptionsValue.MaxEmailsPerWindow)
            throw new TooManyInvitationEmailsException(user.Id, invitationOptionsValue.MaxEmailsPerWindow, invitationOptionsValue.RateLimitWindowMinutes);

        var participant = @event.InviteParticipant(currentUserService.UserId!.Value, user.Id, now);

        await eventRepository.ApplyAsync(@event, cancellationToken);

        var inviteLink = $"{frontendOptions.Value.BaseUrl}/invite/{@event.InviteLinkToken}";
        // Le titre est un texte libre choisi par le créateur de l'événement (pas de restriction de
        // caractères, cf. CreateEventCommandValidator) : encodage HTML nécessaire avant interpolation
        // dans le corps de l'email pour éviter une injection HTML vers l'invité destinataire.
        var encodedTitle = WebUtility.HtmlEncode(@event.Title);
        await emailSender.SendAsync(
            user.Email.Value,
            $"Invitation à « {@event.Title} »",
            $"<p>Vous avez été invité(e) à l'événement « {encodedTitle} » sur EventCo.</p>"
            + $"<p><a href=\"{inviteLink}\">{inviteLink}</a></p>",
            cancellationToken);

        return new InviteParticipantResult(
            @event.Id,
            user.Id,
            user.Email.Value,
            user.DisplayName,
            participant.Role.ToString(),
            participant.InvitedAt,
            participant.ParticipationStatus.ToString(),
            fileStorage.GetAvatarUrl(user));
    }

    private static string DisplayNameFromEmail(string email) => email[..email.IndexOf('@')];
}
