using Scalar.AspNetCore;
using SoatTechChallenge.Application;
using SoatTechChallenge.Extensions;
using SoatTechChallenge.Infrastucture;
using SoatTechChallenge.Middlewares;

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

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapScalarApiReference();

await app.RunAsync();