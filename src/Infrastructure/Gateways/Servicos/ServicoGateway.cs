using Domain.Servicos;
using Domain.Servicos.Gateways;
using Microsoft.EntityFrameworkCore;
using SoatTechChallenge.Infrastucture.Database;

namespace SoatTechChallenge.Infrastucture.Gateways.Servicos;

public class ServicoGateway : IServicoGateway
{
    private readonly SoatTechChallengeDbContext _db;
    public ServicoGateway(SoatTechChallengeDbContext db) => _db = db;

    public Task<Servico?> BuscarPorId(Guid id, CancellationToken ct) => _db.Servico.FirstOrDefaultAsync(s => s.Id == id, ct);  
    
    public async Task<Dictionary<Guid, Servico>> BuscarPorIds(IReadOnlyList<Guid> ids, CancellationToken ct) =>
        await _db.Servico
            .AsNoTracking()
            .Where(s => ids.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, ct);
    
    public async Task Salvar(Servico servico, CancellationToken ct)
    {
        _db.Servico.Add(servico);
        await _db.SaveChangesAsync(ct);
    }

    public async Task Atualizar(Servico servico, CancellationToken ct) => await _db.SaveChangesAsync(ct);

    public async Task Remover(Servico servico, CancellationToken ct)
    {
        _db.Servico.Remove(servico);
        await _db.SaveChangesAsync(ct);
    }
}