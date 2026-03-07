using Microsoft.EntityFrameworkCore;
using SoatTechChallenge.Clientes;
using SoatTechChallenge.Domain.Clientes;
using SoatTechChallenge.Domain.Clientes.Veiculos;
using SoatTechChallenge.Domain.Produtos;
using SoatTechChallenge.Domain.Servicos;

namespace SoatTechChallenge.Infrastructure.Database;

public class SoatTechChallengeDbContext : DbContext
{
    public SoatTechChallengeDbContext(DbContextOptions<SoatTechChallengeDbContext> options) : base(options) { }

    public DbSet<Cliente> Cliente { get; set; }
    public DbSet<ClienteVeiculo> ClienteVeiculo { get; set; }
    
    public DbSet<Servico> Servico { get; set; }
    public DbSet<Produto> Produto { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(SoatTechChallengeDbContext).Assembly);
    }
}