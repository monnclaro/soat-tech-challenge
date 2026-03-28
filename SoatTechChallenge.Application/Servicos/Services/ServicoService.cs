using Microsoft.EntityFrameworkCore;
using SoatTechChallenge.Application.Common.DTOs;
using SoatTechChallenge.Application.Common.Interfaces;
using SoatTechChallenge.Application.Servicos.DTOs;
using SoatTechChallenge.Application.Servicos.DTOs.Requests;
using SoatTechChallenge.Application.Servicos.DTOs.Responses;
using SoatTechChallenge.Domain.Common.Exceptions;
using SoatTechChallenge.Domain.Common.Interfaces;
using SoatTechChallenge.Domain.OrdensServico.Servicos;
using SoatTechChallenge.Domain.OrdensServico.Servicos.Enums;
using SoatTechChallenge.Domain.Servicos;

namespace SoatTechChallenge.Application.Servicos.Services;

public class ServicoService : IServicoService, IScopedService
{
    private readonly IRepository<Servico> _repository;
    private readonly IRepository<OrdemServicoServico> _ordemServicoServicoRepository;
    
    private static ServicoResponse MapToResponse(Servico s) => new(s.Id, s.Nome, s.Descricao, s.Valor);

    public ServicoService(
        IRepository<Servico> repository,
        IRepository<OrdemServicoServico> ordemServicoServicoRepository)
    {
        _repository = repository;
        _ordemServicoServicoRepository = ordemServicoServicoRepository;
    }

    public async Task<ServicoResponse> Buscar(Guid id)
    {
        var resultado = await _repository.GetQueryable().FirstOrDefaultAsync(l => l.Id == id);
        if (resultado is null) throw new NotFoundException("Servico não encontrado.");

        return MapToResponse(resultado);
    }

    public async Task<PagedResponse<ServicoResponse>> BuscarListaPaginada(PagedRequest request)
    {
        var query = _repository.GetQueryable().AsNoTracking()
            .OrderBy(l => l.Nome)
            .Select(s => new ServicoResponse(s.Id, s.Nome, s.Descricao, s.Valor));

        var total = await query.CountAsync();
        var resultado = await query
            .Skip((request.Pagina - 1) * request.Tamanho)
            .Take(request.Tamanho)
            .ToListAsync();

        return new PagedResponse<ServicoResponse>(resultado, total, request.Pagina, request.Tamanho);
    }
    
    public async Task<List<TempoMedioExecucaoServicosResponse>> BuscarTempoMedioExecucao()
    {
        var resultado = await (
            from ordemServico in _ordemServicoServicoRepository.GetQueryable().AsNoTracking()
            join servico in _repository.GetQueryable().AsNoTracking() on ordemServico.IdServico equals servico.Id
            where ordemServico.Status == StatusOrdemServicoServico.ExecucaoFinalizada
            group ordemServico by new 
            { 
                servico.Id, 
                servico.Nome 
            } into g
            select new TempoMedioExecucaoServicosResponse
            {
                Servico = g.Key.Nome,
                TempoMedioMinutos = g.Average(o => (o.DataFinalizacaoExecucao!.Value - o.DataInicioExecucao!.Value).TotalMinutes),
                TempoMinimoMinutos = g.Min(o => (o.DataFinalizacaoExecucao!.Value - o.DataInicioExecucao!.Value).TotalMinutes),
                TempoMaximoMinutos = g.Max(o => (o.DataFinalizacaoExecucao!.Value - o.DataInicioExecucao!.Value).TotalMinutes),
            }
        ).ToListAsync();

        return resultado;
    }

    public async Task<ServicoResponse> Inserir(InserirServicoRequest request)
    {
        var servico = new Servico();
        servico.Inserir(request.Nome, request.Descricao, request.Valor);

        await _repository.InsertAsync(servico);
        return MapToResponse(servico);
    }

    public async Task<ServicoResponse> Atualizar(Guid id, AtualizarServicoRequest request)
    {
        var servico = await _repository.GetQueryable().FirstOrDefaultAsync(l => l.Id == id);
        if (servico is null) throw new NotFoundException("Servico não encontrado.");

        servico.Atualizar(request.Nome, request.Descricao, request.Valor);
        await _repository.UpdateAsync(servico);

        return MapToResponse(servico);
    }

    public async Task Remover(Guid id)
    {
        var servico = await _repository.GetQueryable().FirstOrDefaultAsync(l => l.Id == id);
        if (servico is null) return;

        await _repository.DeleteAsync(servico.Id);
    }
}