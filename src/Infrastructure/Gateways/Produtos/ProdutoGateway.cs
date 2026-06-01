using Domain.Produtos;
using Domain.Produtos.Gateways;
using Microsoft.EntityFrameworkCore;
using SharedKernel.DTOs;
using SoatTechChallenge.Infrastucture.Database;

namespace SoatTechChallenge.Infrastucture.Gateways.Produtos;

public class ProdutoGateway : IProdutoGateway
{
    private readonly SoatTechChallengeDbContext _db;

    public ProdutoGateway(SoatTechChallengeDbContext db) => _db = db;

    public Task<Produto?> BuscarPorId(Guid id, CancellationToken ct) => _db.Produto.FirstOrDefaultAsync(p => p.Id == id, ct);
    
    public async Task<Dictionary<Guid, Produto>> BuscarDicionarioPorIds(IReadOnlyList<Guid> ids, CancellationToken ct) =>
        await _db.Produto
            .AsNoTracking()
            .Where(p => ids.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);
    
    public Task<IReadOnlyList<Produto>> BuscarPorIds(IReadOnlyList<Guid> ids, CancellationToken ct) =>
        _db.Produto
           .Where(p => ids.Contains(p.Id))
           .ToListAsync(ct)
           .ContinueWith(t => (IReadOnlyList<Produto>)t.Result, ct);

    public async Task<(IReadOnlyList<Produto>, int)> BuscarPaginado(string? filtro, PagedRequest p, CancellationToken ct)
    {
        var query = _db.Produto.AsNoTracking().OrderBy(x => x.Nome);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((p.Pagina - 1) * p.Tamanho).Take(p.Tamanho).ToListAsync(ct);
        return (items, total);
    }

    public async Task Salvar(Produto produto, CancellationToken ct)
    {
        _db.Produto.Add(produto);
        await _db.SaveChangesAsync(ct);
    }

    public async Task Atualizar(Produto produto, CancellationToken ct) =>
        await _db.SaveChangesAsync(ct);

    public async Task AtualizarLote(IReadOnlyList<Produto> produtos, CancellationToken ct) =>
        await _db.SaveChangesAsync(ct);

    public async Task Remover(Produto produto, CancellationToken ct)
    {
        _db.Produto.Remove(produto);
        await _db.SaveChangesAsync(ct);
    }
}
