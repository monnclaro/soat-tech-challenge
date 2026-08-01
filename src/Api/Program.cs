using Api.Extensions;
using Api.Middlewares;
using Application;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Formatting.Compact;
using SoatTechChallenge.Infrastucture;

// Logs estruturados em JSON (CLEF) no stdout — o New Relic Kubernetes
// integration (infra-k8s) coleta via Fluent Bit e correlaciona pelo campo
// CorrelationId injetado por CorrelationIdMiddleware.
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console(new RenderedCompactJsonFormatter())
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
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