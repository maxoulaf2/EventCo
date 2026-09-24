using EventCo.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventCo.Infrastructure.Persistence;

public class EventCoDbContext(DbContextOptions<EventCoDbContext> options) : DbContext(options)
{
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<LoginCodeEntity> LoginCodes => Set<LoginCodeEntity>();
    public DbSet<EventEntity> Events => Set<EventEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventCoDbContext).Assembly);
    }
}
