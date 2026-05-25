using Domain.Clientes;
using Domain.Clientes.Gateways;
using Microsoft.EntityFrameworkCore;
using SharedKernel.DTOs;
using SoatTechChallenge.Infrastucture.Database;

namespace SoatTechChallenge.Infrastucture.Gateways.Clientes;

public class ClienteGateway : IClienteGateway
{
    private readonly SoatTechChallengeDbContext _db;

    public ClienteGateway(SoatTechChallengeDbContext db) => _db = db;

    public Task<Cliente?> BuscarPorId(Guid id, CancellationToken ct) => _db.Cliente.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<Cliente?> BuscarComVeiculos(Guid id, CancellationToken ct) =>
        _db.Cliente
            .Include(c => c.Veiculos)
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    
    public Task<bool> ExisteComDocumento(string documento, CancellationToken ct) =>
        _db.Cliente.AsNoTracking().AnyAsync(c => c.Documento == documento, ct);

    public async Task<(IReadOnlyList<Cliente>, int)> BuscarPaginado(PagedRequest p, CancellationToken ct)
    {
        var query = _db.Cliente.AsNoTracking().OrderBy(c => c.DataCriacao);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((p.Pagina - 1) * p.Tamanho).Take(p.Tamanho).ToListAsync(ct);
        return (items, total);
    }

    public async Task Salvar(Cliente cliente, CancellationToken ct)
    {
        _db.Cliente.Add(cliente);
        await _db.SaveChangesAsync(ct);
    }

    public async Task Atualizar(Cliente cliente, CancellationToken ct) =>
        await _db.SaveChangesAsync(ct);

    public async Task Remover(Cliente cliente, CancellationToken ct)
    {
        _db.Cliente.Remove(cliente);
        await _db.SaveChangesAsync(ct);
    }
}
