using System.Reflection;
using Application.Common.Markers;
using Application.Servicos.Queries.BuscarServico;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assemblies = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll")
            .Select(Assembly.LoadFrom)
            .ToArray();

        services.Scan(scan => scan
            .FromAssemblyOf<BuscarServicoUseCase>()
            .AddClasses(c => c.AssignableTo<IUseCase>())
            .AsSelf()
            .WithScopedLifetime());

        return services;
    }
}