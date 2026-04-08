using Microsoft.EntityFrameworkCore;
using SoatTechChallenge.Infrastucture.Database;
using SoatTechChallenge.Infrastucture.Seeders;

namespace SoatTechChallenge.Extensions;

public static class DatabaseExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<SoatTechChallengeDbContext>();
        await db.Database.MigrateAsync();

        var seeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeeder>();
        await seeder.SeedAsync();
    }
}