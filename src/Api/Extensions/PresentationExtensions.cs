using Api.Extensions.Markers;
using SharedKernel;

namespace Api.Extensions;

public static class PresentationExtensions
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddControllers();
        
        services.Scan(scan => scan
            .FromApplicationDependencies()
            .AddClasses(c => c.AssignableTo<IPresenter>())
            .AsSelfWithInterfaces()
            .WithScopedLifetime());
        
        services.Scan(scan => scan
            .FromApplicationDependencies()
            .AddClasses(c => c.AssignableTo<IScoped>())
            .AsSelfWithInterfaces()
            .WithScopedLifetime());
        
        services.AddOpenApi(options =>
        {
            /*options.AddDocumentTransformer((document, _, _) =>
            {
                foreach (var path in document.Paths)
                foreach (var operation in path.Value.Operations)
                foreach (var param in operation.Value.Parameters ?? [])
                {
                    if (param.Schema?.Format == "uuid")
                    {
                        param.Example = new OpenApiString("550e8400-e29b-41d4-a716-446655440000");
                    }
                }

                return Task.CompletedTask;
            });*/
        });

        return services;
    }
}