using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SoatTechChallenge.Infrastucture.Database.Helpers;

namespace SoatTechChallenge.Infrastucture.Database;

public class SoatTechChallengeDbContextFactory : IDesignTimeDbContextFactory<SoatTechChallengeDbContext>
{
    public SoatTechChallengeDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<SoatTechChallengeDbContext>()
            .UseNpgsql("Host=localhost;Database=soat;Username=postgres;Password=postgres")
            .Options;

        return new SoatTechChallengeDbContext(options, new NoopDomainEventsDispatcher());
    }
}