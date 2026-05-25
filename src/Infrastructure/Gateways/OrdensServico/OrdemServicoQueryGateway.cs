using Application.OrdensServico.Queries;
using Application.OrdensServico.UseCases;
using Domain.OrdensServico.Enums;
using Microsoft.EntityFrameworkCore;
using SharedKernel.DTOs;
using SoatTechChallenge.Infrastucture.Database;

namespace SoatTechChallenge.Infrastucture.Gateways.OrdensServico;

public class OrdemServicoQueryGateway : IOrdemServicoQueryGateway
{
    private readonly SoatTechChallengeDbContext _db;

    public OrdemServicoQueryGateway(SoatTechChallengeDbContext db) => _db = db;
    
    public async Task<OrdemServicoOutput?> BuscarComDetalhes(Guid id, CancellationToken ct)
    {
        var query = from os in _db.OrdemServico.AsNoTracking().Where(o => o.Id == id)
                    join c in _db.Cliente.AsNoTracking() on os.IdCliente equals c.Id
                    join v in _db.Veiculo.AsNoTracking() on os.IdVeiculo equals v.Id
                    select new OrdemServicoOutput(
                        os.Id,
                        new OrdemServicoClienteOutput(c.Id, c.Nome, c.Documento),
                        new OrdemServicoVeiculoOutput(v.Id, v.Placa, v.Marca, v.Modelo, v.Ano),
                        os.DataCriacao,
                        os.DataInicioExecucao,
                        os.DataFinalizacao,
                        os.Status.ToString(),
                        os.ValorTotal,
                        os.Servicos.Select(s => new OrdemServicoServicoOutput(
                            s.Id, s.IdServico, s.NomeServico, s.Valor, s.Status.ToString())).ToList(),
                        os.Produtos.Select(p => new OrdemServicoProdutoOutput(
                            p.Id, p.IdProduto, p.NomeProduto, p.ValorUnitario, p.Quantidade)).ToList());

        return await query.FirstOrDefaultAsync(ct);
    }

    public Task<OrdemServicoStatusOutput?> BuscarStatus(Guid id, CancellationToken ct) =>
        _db.OrdemServico.AsNoTracking()
           .Where(o => o.Id == id)
           .Select(o => new OrdemServicoStatusOutput(o.Id, o.Status.ToString()))
           .FirstOrDefaultAsync(ct);

    public async Task<(IReadOnlyList<OrdemServicoOutput> Items, int Total)> BuscarPaginado(
        PagedRequest p, CancellationToken ct)
    {
        var query = from os in _db.OrdemServico.AsNoTracking()
                    where os.Status != StatusOrdemServico.Finalizada
                          && os.Status != StatusOrdemServico.Entregue
                    join c in _db.Cliente.AsNoTracking() on os.IdCliente equals c.Id
                    join v in _db.Veiculo.AsNoTracking() on os.IdVeiculo equals v.Id
                    orderby
                        os.Status == StatusOrdemServico.EmExecucao ? 1 :
                        os.Status == StatusOrdemServico.AguardandoAprovacao ? 2 :
                        os.Status == StatusOrdemServico.EmDiagnostico ? 3 :
                        os.Status == StatusOrdemServico.Recebida ? 4 : 99,
                        os.DataCriacao
                    select new OrdemServicoOutput(
                        os.Id,
                        new OrdemServicoClienteOutput(c.Id, c.Nome, c.Documento),
                        new OrdemServicoVeiculoOutput(v.Id, v.Placa, v.Marca, v.Modelo, v.Ano),
                        os.DataCriacao,
                        os.DataInicioExecucao,
                        os.DataFinalizacao,
                        os.Status.ToString(),
                        os.ValorTotal,
                        os.Servicos.Select(s => new OrdemServicoServicoOutput(
                            s.Id, s.IdServico, s.NomeServico, s.Valor, s.Status.ToString())).ToList(),
                        os.Produtos.Select(pr => new OrdemServicoProdutoOutput(
                            pr.Id, pr.IdProduto, pr.NomeProduto, pr.ValorUnitario, pr.Quantidade)).ToList());

        var total = await query.CountAsync(ct);
        var items = await query.Skip((p.Pagina - 1) * p.Tamanho).Take(p.Tamanho).ToListAsync(ct);
        return (items, total);
    }

    public async Task<(IReadOnlyList<OrdemServicoPorDocumentoOutput> Items, int Total)> BuscarPaginadoPorDocumento(
        string documento, PagedRequest p, CancellationToken ct)
    {
        var idsClientes = await _db.Cliente
            .AsNoTracking()
            .Where(c => c.Documento == documento)
            .Select(c => c.Id)
            .ToListAsync(ct);

        if (!idsClientes.Any())
            return ([], 0);

        var query = from os in _db.OrdemServico.AsNoTracking()
            where idsClientes.Contains(os.IdCliente)
            join c in _db.Cliente.AsNoTracking() on os.IdCliente equals c.Id
            join v in _db.Veiculo.AsNoTracking() on os.IdVeiculo equals v.Id
            orderby os.DataCriacao
            select new OrdemServicoPorDocumentoOutput(
                os.Status.ToString(),
                new OrdemServicoClientePorDocumentoOutput(c.Nome, c.Documento),
                new OrdemServicoVeiculoPorDocumentoOutput(v.Placa, v.Marca, v.Modelo, v.Ano),
                os.Servicos.Select(s => new OrdemServicoServicoPorDocumentoOutput(
                    s.NomeServico, s.Status.ToString())).ToList());

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((p.Pagina - 1) * p.Tamanho)
            .Take(p.Tamanho)
            .ToListAsync(ct);

        return (items, total);
    }
}