using EventCo.Domain.Auth;
using EventCo.Domain.Events;
using EventCo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace EventCo.Infrastructure.Persistence;

public class EventCoDbContext(DbContextOptions<EventCoDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<MagicLinkToken> MagicLinkTokens => Set<MagicLinkToken>();
    public DbSet<Event> Events => Set<Event>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventCoDbContext).Assembly);
    }
}
