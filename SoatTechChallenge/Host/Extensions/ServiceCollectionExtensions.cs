using System.Reflection;
using SoatTechChallenge.Host.Common.Services;

namespace SoatTechChallenge.Host.Extensions;

public static class ServiceCollectionExtensions
{

    public static IServiceCollection AddAllServices(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        var types = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass 
                        && !t.IsAbstract 
                        && !t.IsGenericTypeDefinition);

        foreach (var impl in types)
        {
            if (typeof(IScopedService).IsAssignableFrom(impl))
                Register(services, impl, ServiceLifetime.Scoped);
            else if (typeof(ISingletonService).IsAssignableFrom(impl))
                Register(services, impl, ServiceLifetime.Singleton);
            else if (typeof(ITransientService).IsAssignableFrom(impl))
                Register(services, impl, ServiceLifetime.Transient);
        }

        return services;
    }

    private static readonly HashSet<Type> _markerInterfaces =
    [
        typeof(IScopedService),
        typeof(ISingletonService),
        typeof(ITransientService)
    ];

    private static void Register(
        IServiceCollection services,
        Type implementation,
        ServiceLifetime lifetime)
    {
        var interfaces = implementation
            .GetInterfaces()
            .Where(i => !_markerInterfaces.Contains(i))
            .ToList();

        var registrations = interfaces.Any()
            ? interfaces.Select(i =>
                new ServiceDescriptor(i, implementation, lifetime))
            : [new ServiceDescriptor(implementation, implementation, lifetime)];

        foreach (var sd in registrations)
            services.Add(sd);
    }
}