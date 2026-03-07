using Microsoft.EntityFrameworkCore;
using SoatTechChallenge.Clientes;
using SoatTechChallenge.Clientes.Veiculos;

namespace SoatTechChallenge.Infrastructure.Database;

public class SoatTechChallengeDbContext : DbContext
{
    public SoatTechChallengeDbContext(DbContextOptions<SoatTechChallengeDbContext> options) : base(options) { }

    public DbSet<Cliente> Cliente { get; set; }
    public DbSet<ClienteVeiculo> ClienteVeiculo { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(SoatTechChallengeDbContext).Assembly);
    }
}