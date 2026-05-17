using Domain.OrdensServico;
using Domain.OrdensServico.Gateways;
using Microsoft.EntityFrameworkCore;
using SoatTechChallenge.Infrastucture.Database;

namespace SoatTechChallenge.Infrastucture.Gateways.OrdensServico;

public class OrdemServicoGateway : IOrdemServicoGateway
{
    private readonly SoatTechChallengeDbContext _db;

    public OrdemServicoGateway(SoatTechChallengeDbContext db) => _db = db;

    public Task<OrdemServico?> BuscarPorId(Guid id, CancellationToken ct) =>
        _db.OrdemServico.FirstOrDefaultAsync(o => o.Id == id, ct);

    public Task<OrdemServico?> BuscarComServicos(Guid id, CancellationToken ct) =>
        _db.OrdemServico
           .Include(o => o.Servicos)
           .FirstOrDefaultAsync(o => o.Id == id, ct);

    public Task<OrdemServico?> BuscarComProdutos(Guid id, CancellationToken ct) =>
        _db.OrdemServico
           .Include(o => o.Produtos)
           .FirstOrDefaultAsync(o => o.Id == id, ct);

    public Task<OrdemServico?> BuscarComServicosProdutos(Guid id, CancellationToken ct) =>
        _db.OrdemServico
           .AsSplitQuery()
           .Include(o => o.Servicos)
           .Include(o => o.Produtos)
           .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task Salvar(OrdemServico ordemServico, CancellationToken ct)
    {
        _db.OrdemServico.Add(ordemServico);
        await _db.SaveChangesAsync(ct);
    }

    public async Task Atualizar(OrdemServico ordemServico, CancellationToken ct) =>
        await _db.SaveChangesAsync(ct);

    public async Task Remover(OrdemServico ordemServico, CancellationToken ct)
    {
        _db.OrdemServico.Remove(ordemServico);
        await _db.SaveChangesAsync(ct);
    }
}