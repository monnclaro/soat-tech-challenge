using Api.Extensions;
using Api.Middlewares;
using Application;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Formatting.Compact;
using SoatTechChallenge.Infrastucture;

var builder = WebApplication.CreateBuilder(args);

// Logs estruturados em JSON (CLEF) no stdout — o New Relic Kubernetes
// integration (infra-k8s) coleta via Fluent Bit e correlaciona pelo campo
// CorrelationId injetado por CorrelationIdMiddleware.
//
// ReadFrom.Configuration precisa vir depois do builder existir (é o que dá
// acesso ao appsettings.json) — sem isso, o filtro "Microsoft.AspNetCore":
// "Warning" do appsettings.json nunca era aplicado de verdade, e logs de
// framework tipo "Request starting"/"Request finished" (Information)
// vazavam sem parar.
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(new RenderedCompactJsonFormatter())
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services
    .AddApplication()
    .AddPresentation()
    .AddInfrastructure(builder.Configuration)
    .AddJwtAuthentication(builder.Configuration)
    .AddHealthChecks();

var app = builder.Build();
await app.InitializeDatabaseAsync();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Use(async (context, next) =>
{
    context.Request.EnableBuffering();
    await next();
});

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapScalarApiReference();
app.MapHealthChecks("/health");

await app.RunAsync();