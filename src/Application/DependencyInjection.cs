using System.Reflection;
using Application.Common.Interfaces;
using Application.Login.Controllers;
using Application.Servicos.Queries.BuscarServico;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Interfaces;

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

        services.Scan(scan => scan
            .FromAssemblyOf<LoginController>()
            .AddClasses(c => c.AssignableTo<IScoped>())
            .AsSelf()
            .WithScopedLifetime());

        services.Scan(scan => scan
            .FromAssemblyOf<BuscarServicoUseCase>()
            // publicOnly: false — os handlers de domain event (ex.: OrdemServicoEventHandler)
            // são internal ao assembly Application; o Scrutor só escaneia classes públicas por padrão.
            .AddClasses(c => c.AssignableTo(typeof(IDomainEventHandler<>)), publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}