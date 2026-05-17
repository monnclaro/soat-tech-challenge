using Application.Login.UseCases.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SoatTechChallenge.Infrastucture.Database;
using SoatTechChallenge.Infrastucture.DomainEvents;
using SoatTechChallenge.Infrastucture.Security;
using SoatTechChallenge.Infrastucture.Security.BCrypt;
using SoatTechChallenge.Infrastucture.Security.Jwt;
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
        services.AddScoped<ITokenProvider, JwtTokenProvider>();
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        
        services.Scan(scan => scan
            .FromApplicationDependencies()
            .AddClasses(c => c.Where(t => t.Name.EndsWith("Gateway")))
            .AsImplementedInterfaces()
            .WithScopedLifetime());
    
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