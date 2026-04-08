using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SoatTechChallenge.Domain.Common.Interfaces;
using SoatTechChallenge.Infrastucture.Database;
using SoatTechChallenge.Infrastucture.DomainEvents;
using SoatTechChallenge.Infrastucture.Persistence;
using SoatTechChallenge.Infrastucture.Seeders;

namespace SoatTechChallenge.Infrastucture;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services
            .AddServices()
            .AddDatabase(configuration);

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>();
        
        services.AddScoped<IDatabaseSeeder, DatabaseSeeder>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("Default");

        services.AddDbContext<SoatTechChallengeDbContext>(options =>
            options.UseNpgsql(connectionString,
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName)));

        return services;
    }
}