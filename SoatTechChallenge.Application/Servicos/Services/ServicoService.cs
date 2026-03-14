using Microsoft.EntityFrameworkCore;
using SoatTechChallenge.Application.Common.Interfaces;
using SoatTechChallenge.Application.Servicos.DTOs;
using SoatTechChallenge.Domain.Common.Exceptions;
using SoatTechChallenge.Domain.Common.Interfaces;
using SoatTechChallenge.Domain.Servicos;
using SoatTechChallenge.Host.Common.DTOs;
using SoatTechChallenge.Host.Controllers.Servicos.DTOs;

namespace SoatTechChallenge.Application.Servicos.Services;

public class ServicoService : IServicoService, IScopedService
{
    private readonly IRepository<Servico> _repository;

    private static ServicoResponse MapToResponse(Servico s) => new(s.Id, s.Nome, s.Descricao, s.Valor);

    public ServicoService(IRepository<Servico> repository)
    {
        _repository = repository;
    }

    public async Task<ServicoResponse> Buscar(Guid id)
    {
        var resultado = await _repository.GetQueryable().FirstOrDefaultAsync(l => l.Id == id);
        if (resultado is null) throw new NotFoundException($"Servico de id: {id} não encontrado.");

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
        if (servico is null) throw new NotFoundException($"Servico de id: {id} não encontrado.");

        servico.Atualizar(request.Nome, request.Descricao, request.Valor);
        await _repository.UpdateAsync(servico);

        return MapToResponse(servico);
    }

    public async Task Remover(Guid id)
    {
        var servico = await _repository.GetQueryable().FirstOrDefaultAsync(l => l.Id == id);
        if (servico is null) throw new NotFoundException($"Servico de id: {id} não encontrado.");

        await _repository.DeleteAsync(servico.Id);
    }
}