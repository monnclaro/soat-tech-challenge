using Microsoft.EntityFrameworkCore;
using SoatTechChallenge.Clientes;
using SoatTechChallenge.Domain.Clientes;
using SoatTechChallenge.Domain.Clientes.Veiculos;
using SoatTechChallenge.Domain.OrdensServico;
using SoatTechChallenge.Domain.OrdensServico.Produtos;
using SoatTechChallenge.Domain.OrdensServico.Services;
using SoatTechChallenge.Domain.OrdensServico.Servicos;
using SoatTechChallenge.Domain.Produtos;
using SoatTechChallenge.Domain.Servicos;
using SoatTechChallenge.Domain.Usuarios;

namespace SoatTechChallenge.Infrastructure.Database;

public class SoatTechChallengeDbContext : DbContext
{
    public SoatTechChallengeDbContext(DbContextOptions<SoatTechChallengeDbContext> options) : base(options) { }

    public DbSet<OrdemServico> OrdemServico { get; set; }
    public DbSet<OrdemServicoServico> OrdemServicoServico { get; set; }
    public DbSet<OrdemServicoProduto> OrdemServicoProduto { get; set; }

    public DbSet<Cliente> Cliente { get; set; }
    public DbSet<ClienteVeiculo> ClienteVeiculo { get; set; }
    
    public DbSet<Servico> Servico { get; set; }
    public DbSet<Produto> Produto { get; set; }
    
    public DbSet<Usuario> Usuario { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(SoatTechChallengeDbContext).Assembly);
    }
}