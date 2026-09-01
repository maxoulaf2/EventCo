using EventCo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventCo.Application.Tests.Support;

public sealed class ApplicationTestHostBuilder
{
    private readonly ServiceCollection _services = new();

    public ApplicationTestHostBuilder()
    {
        _services.AddApplication();
        _services.AddDbContext<EventCoDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
    }

    public IServiceCollection Services => _services;

    public IServiceProvider Build() => _services.BuildServiceProvider();
}
