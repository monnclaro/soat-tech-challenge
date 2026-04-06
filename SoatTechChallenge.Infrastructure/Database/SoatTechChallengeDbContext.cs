using Microsoft.EntityFrameworkCore;
using SharedKernel;
using SoatTechChallenge.Domain.Clientes;
using SoatTechChallenge.Domain.Clientes.Veiculos;
using SoatTechChallenge.Domain.OrdensServico;
using SoatTechChallenge.Domain.OrdensServico.Produtos;
using SoatTechChallenge.Domain.OrdensServico.Servicos;
using SoatTechChallenge.Domain.Produtos;
using SoatTechChallenge.Domain.Servicos;
using SoatTechChallenge.Domain.Usuarios;
using SoatTechChallenge.Infrastucture.DomainEvents;

namespace SoatTechChallenge.Infrastucture.Database;

public class SoatTechChallengeDbContext : DbContext
{
    private readonly IDomainEventsDispatcher _dispatcher;

    public SoatTechChallengeDbContext(
        DbContextOptions<SoatTechChallengeDbContext> options,
        IDomainEventsDispatcher dispatcher) : base(options)
    {
        _dispatcher = dispatcher;
    }

    public DbSet<OrdemServico> OrdemServico { get; set; }
    public DbSet<OrdemServicoServico> OrdemServicoServico { get; set; }
    public DbSet<OrdemServicoProduto> OrdemServicoProduto { get; set; }
    public DbSet<Cliente> Cliente { get; set; }
    public DbSet<Veiculo> Veiculo { get; set; }
    public DbSet<Servico> Servico { get; set; }
    public DbSet<Produto> Produto { get; set; }
    public DbSet<Usuario> Usuario { get; set; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        int result = await base.SaveChangesAsync(cancellationToken);
        
        await PublishDomainEventsAsync();
        
        return result;
    }

    private async Task PublishDomainEventsAsync()
    {
        var domainEvents = ChangeTracker
            .Entries<Entity>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                List<IDomainEvent> domainEvents = entity.DomainEvents;

                entity.ClearDomainEvents();

                return domainEvents;
            })
            .ToList();

        await _dispatcher.DispatchAsync(domainEvents);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(SoatTechChallengeDbContext).Assembly);
    }
}