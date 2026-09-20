using EventCo.Domain.Auth;
using EventCo.Domain.ValueObjects;

namespace EventCo.Application.Common.Interfaces;

public interface IMagicLinkTokenRepository: IRepository<MagicLinkToken>
{
    Task<MagicLinkToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken);

    Task<int> CountCreatedSinceAsync(Email email, DateTime since, CancellationToken cancellationToken);
}
