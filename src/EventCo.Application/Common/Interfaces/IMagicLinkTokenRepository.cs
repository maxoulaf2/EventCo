using EventCo.Domain.Auth;

namespace EventCo.Application.Common.Interfaces;

public interface IMagicLinkTokenRepository
{
    Task AddAsync(MagicLinkToken token, CancellationToken cancellationToken);

    Task<MagicLinkToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken);

    Task UpdateAsync(MagicLinkToken token, CancellationToken cancellationToken);
}
