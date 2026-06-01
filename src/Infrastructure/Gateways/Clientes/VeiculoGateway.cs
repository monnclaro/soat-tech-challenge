using Domain.Clientes.Veiculos;
using Domain.Clientes.Veiculos.Gateways;
using Microsoft.EntityFrameworkCore;
using SharedKernel.DTOs;
using SoatTechChallenge.Infrastucture.Database;

namespace SoatTechChallenge.Infrastucture.Gateways.Clientes;

public class VeiculoGateway : IVeiculoGateway
{
    private readonly SoatTechChallengeDbContext _db;

    public VeiculoGateway(SoatTechChallengeDbContext db) => _db = db;

    public Task<Veiculo?> BuscarPorId(Guid id, CancellationToken ct) =>
        _db.Veiculo.FirstOrDefaultAsync(v => v.Id == id, ct);
    
    public Task<Veiculo?> BuscarPorPlaca(string placa, CancellationToken ct) =>
        _db.Veiculo.FirstOrDefaultAsync(v => v.Placa == placa, ct);

    public Task<bool> ExisteComPlaca(string placa, CancellationToken ct) =>
        _db.Veiculo.AsNoTracking().AnyAsync(v => v.Placa == placa, ct);

    public Task<bool> ExisteComPlacaExcetoId(string placa, Guid idVeiculo, CancellationToken ct) =>
        _db.Veiculo.AsNoTracking().AnyAsync(v => v.Placa == placa && v.Id != idVeiculo, ct);

    public async Task<(IReadOnlyList<Veiculo>, int)> BuscarPaginadoPorCliente(Guid idCliente, PagedRequest p, CancellationToken ct)
    {
        var query = _db.Veiculo.AsNoTracking()
            .Where(v => v.IdCliente == idCliente)
            .OrderBy(v => v.DataCriacao);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((p.Pagina - 1) * p.Tamanho).Take(p.Tamanho).ToListAsync(ct);
        return (items, total);
    }

    public async Task Inserir(Veiculo veiculo, CancellationToken ct)
    {
        _db.Veiculo.Add(veiculo);
        await _db.SaveChangesAsync(ct);
    }

    public async Task Atualizar(Veiculo veiculo, CancellationToken ct) =>
        await _db.SaveChangesAsync(ct);

    public async Task Remover(Veiculo veiculo, CancellationToken ct)
    {
        _db.Veiculo.Remove(veiculo);
        await _db.SaveChangesAsync(ct);
    }
}
