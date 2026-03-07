using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using SoatTechChallenge.Infrastructure.Common;
using SoatTechChallenge.Infrastructure.Database;
using SoatTechChallenge.Infrastructure.Extensions;
using SoatTechChallenge.Infrastructure.Interfaces;
using SoatTechChallenge.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddDbContext<SoatTechChallengeDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddAllServices(
    typeof(ITransientService).Assembly,
    typeof(IScopedService).Assembly,
    typeof(ISingletonService).Assembly);

var app = builder.Build();
app.MapScalarApiReference();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SoatTechChallengeDbContext>();
    await db.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.Run();