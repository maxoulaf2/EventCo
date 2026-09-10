using EventCo.Domain.Common;

namespace EventCo.Application.Common.Interfaces;

public interface IRepository<TDomainObject> where TDomainObject: Entity
{
     Task ApplyAsync(TDomainObject domainObject, CancellationToken cancellationToken);
}
