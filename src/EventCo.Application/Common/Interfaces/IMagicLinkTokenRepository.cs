using EventCo.Domain.Auth;
using EventCo.Domain.ValueObjects;

namespace EventCo.Application.Common.Interfaces;

public interface IMagicLinkTokenRepository: IRepository<MagicLinkToken>
{
    // Codes encore utilisables (non consommés, non expirés, non bloqués) : plusieurs peuvent coexister
    // si l'utilisateur a redemandé un code, chacun restant valable jusqu'à son expiration.
    Task<IReadOnlyList<MagicLinkToken>> GetUsableByEmailAsync(Email email, DateTime now, CancellationToken cancellationToken);

    Task<int> CountCreatedSinceAsync(Email email, DateTime since, CancellationToken cancellationToken);
}
