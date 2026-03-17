using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using SoatTechChallenge.Application.Common.DTOs;
using SoatTechChallenge.Application.Common.Interfaces;
using SoatTechChallenge.Domain.Common.Interfaces;
using SoatTechChallenge.Infrastucture.Database;
using SoatTechChallenge.Infrastucture.Persistence;
using SoatTechChallenge.Infrastucture.Seeders.Clientes;
using SoatTechChallenge.Infrastucture.Seeders.OrdensServico;
using SoatTechChallenge.Infrastucture.Seeders.Produtos;
using SoatTechChallenge.Infrastucture.Seeders.Servicos;
using SoatTechChallenge.Infrastucture.Seeders.Usuarios;
using SoatTechChallenge.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddDbContext<SoatTechChallengeDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
    
var assemblies = AppDomain.CurrentDomain.GetAssemblies();
var services = assemblies.SelectMany(a => a.GetTypes()).Where(t => t.IsClass && !t.IsAbstract && typeof(IScopedService).IsAssignableFrom(t));
foreach (var service in services)
{
    var interfaces = service.GetInterfaces().Where(i => i != typeof(IScopedService));
    foreach (var iface in interfaces)
    {
        builder.Services.TryAddScoped(iface, service);
    }
}
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration
    .GetSection("JwtSettings")
    .Get<JwtSettings>();

var key = Encoding.UTF8.GetBytes(jwtSettings!.Secret);

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

var app = builder.Build();
app.MapScalarApiReference();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SoatTechChallengeDbContext>();
    await db.Database.MigrateAsync();
    
    await ClienteSeeder.SeedAsync(db);
    await ProdutoSeeder.SeedAsync(db);
    await ServicoSeeder.SeedAsync(db);
    await UsuarioSeeder.SeedAsync(db);
    await OrdensServicoSeeder.SeedAsync(db);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();