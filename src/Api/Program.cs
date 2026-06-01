using Api.Extensions;
using Api.Middlewares;
using Application;
using Scalar.AspNetCore;
using SoatTechChallenge.Infrastucture;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddPresentation()
    .AddInfrastructure(builder.Configuration)
    .AddJwtAuthentication(builder.Configuration);

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

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapScalarApiReference();

await app.RunAsync();