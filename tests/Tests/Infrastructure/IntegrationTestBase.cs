using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SoatTechChallenge.Infrastucture.Database;
using SoatTechChallenge.Infrastucture.Database.Helpers;
using SoatTechChallenge.Infrastucture.DomainEvents;
using Testcontainers.PostgreSql;
using Xunit;

namespace Tests.Infrastructure;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("soattest")
        .WithUsername("soatuser")
        .WithPassword("soatpass")
        .Build();

    protected ServiceProvider Provider { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var services = new ServiceCollection();

        services.AddScoped<IDomainEventsDispatcher, NoopDomainEventsDispatcher>();
        services.AddDbContext<SoatTechChallengeDbContext>(o => o.UseNpgsql(_postgres.GetConnectionString()));

        RegisterServices(services);
        Provider = services.BuildServiceProvider();

        using var scope = Provider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<SoatTechChallengeDbContext>().Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await Provider.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    /// <summary>
    /// Cada classe de teste registra seus gateways/serviços específicos aqui.
    /// </summary>
    protected abstract void RegisterServices(IServiceCollection services);

    protected T GetService<T>(IServiceScope scope) where T : notnull => scope.ServiceProvider.GetRequiredService<T>();

    protected IServiceScope CreateScope() => Provider.CreateScope();
}