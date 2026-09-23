using EventCo.Infrastructure.Persistence;
using EventCo.Infrastructure.Persistence.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace EventCo.Application.Tests.Support;

// Aucun use case n'attribue le flag administrateur (attribution directe en base en production) :
// les tests font de même, via l'entité de persistance plutôt que via l'agrégat User.
public static class AdminUserSeeder
{
    public static async Task<Guid> SeedAsync(IServiceProvider serviceProvider, bool isAdmin = true)
    {
        var dbContext = serviceProvider.GetRequiredService<EventCoDbContext>();
        var userId = Guid.NewGuid();

        dbContext.Users.Add(new UserEntity
        {
            Id = userId,
            Email = $"{userId:N}@example.com",
            DisplayName = isAdmin ? "Administrateur" : "Utilisateur",
            CreatedAt = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            IsAdmin = isAdmin,
        });
        await dbContext.SaveChangesAsync();

        return userId;
    }
}
