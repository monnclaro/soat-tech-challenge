using Application.Servicos.DTOs;
using Application.Servicos.Queries;
using Domain.OrdensServico.Servicos.Enums;
using Domain.Servicos;
using Microsoft.EntityFrameworkCore;
using SharedKernel.DTOs;
using SoatTechChallenge.Infrastucture.Database;

namespace SoatTechChallenge.Infrastucture.Gateways.Servicos;

public class ServicoQueryGateway : IServicoQueryGateway
{
    private readonly SoatTechChallengeDbContext _db;
    public ServicoQueryGateway(SoatTechChallengeDbContext db) => _db = db;
    
    public async Task<(IReadOnlyList<Servico>, int)> BuscarPaginado(string? filtro, PagedRequest p, CancellationToken ct)
    {
        var query = _db.Servico.AsNoTracking().OrderBy(s => s.Nome);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((p.Pagina - 1) * p.Tamanho).Take(p.Tamanho).ToListAsync(ct);
        return (items, total);
    }

    public async Task<IReadOnlyList<TempoMedioExecucaoOutput>> BuscarTempoMedioExecucao(CancellationToken ct)
    {
        var resultado = await (
            from oss in _db.OrdemServicoServico.AsNoTracking()
            join s in _db.Servico.AsNoTracking() on oss.IdServico equals s.Id
            where oss.Status == StatusOrdemServicoServico.ExecucaoFinalizada
            group oss by new { s.Id, s.Nome } into g
            select new TempoMedioExecucaoOutput(
                g.Key.Nome,
                g.Average(o => (o.DataFinalizacaoExecucao!.Value - o.DataInicioExecucao!.Value).TotalMinutes),
                g.Min(o => (o.DataFinalizacaoExecucao!.Value - o.DataInicioExecucao!.Value).TotalMinutes),
                g.Max(o => (o.DataFinalizacaoExecucao!.Value - o.DataInicioExecucao!.Value).TotalMinutes))
        ).ToListAsync(ct);

        return resultado;
    }
}