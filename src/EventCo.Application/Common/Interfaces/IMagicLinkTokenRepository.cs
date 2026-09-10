using EventCo.Domain.Auth;

namespace EventCo.Application.Common.Interfaces;

public interface IMagicLinkTokenRepository: IRepository<MagicLinkToken>
{
    Task<MagicLinkToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken);
}
