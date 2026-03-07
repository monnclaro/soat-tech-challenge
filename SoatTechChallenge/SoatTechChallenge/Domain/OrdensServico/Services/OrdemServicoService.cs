/*using Microsoft.EntityFrameworkCore;
using SoatTechChallenge.Application.DTOs;
using SoatTechChallenge.Host.Controllers.Clientes.DTOs;
using SoatTechChallenge.Host.Controllers.OrdensServico.DTOs;
using SoatTechChallenge.Infrastructure.Common;
using SoatTechChallenge.Infrastructure.Interfaces;
using SoatTechChallenge.Middlewares.Exceptions;

namespace SoatTechChallenge.Domain.OrdensServico.Services;

public class OrdemServicoService : IOrdemServicoService, ITransientService
{
    private readonly IRepository<OrdemServico> _repository;

    private static OrdemServicoResponse MapToResponse(OrdemServico c) => new(c.Id, c.Nome, c.Documento, c.DataCriacao);

    public OrdemServicoService(IRepository<OrdemServico> repository)
    {
        _repository = repository;
    }

    public async Task<OrdemServicoResponse> Buscar(Guid id)
    {
        var resultado = await _repository.Query().AsNoTracking()
            .Include(l => l.Servicos)
            .Include(l => l.Produtos).AsSplitQuery()
            .FirstOrDefaultAsync(l => l.Id == id);
        
        if (resultado is null)
        {
            throw new NotFoundException($"Ordem de serviço de id: {id} não encontrado.");
        }

        return MapToResponse(resultado);
    }

    public async Task<PagedResponse<OrdemServicoResponse>> BuscarListaPaginada(PagedRequest request)
    {
        var resultado = await _repository.Query().AsNoTracking()
            .OrderBy(l => l.DataCriacao)
            .Skip((request.Pagina - 1) * request.Tamanho)
            .Take(request.Tamanho)
            .Select(l => new OrdemServicoResponse(
                l.Id,
                l.Nome,
                l.Documento,
                l.DataCriacao
            ))
            .ToListAsync();

        return new PagedResponse<OrdemServicoResponse>(resultado);
    }

    public async Task<OrdemServicoResponse> Inserir(InserirOrdemServicoRequest request)
    {
        var ordemServico = new OrdemServico();
        ordemServico.Inserir(request);

        await _repository.AddAsync(ordemServico);
        await _repository.SaveChangesAsync();

        return MapToResponse(ordemServico);
    }

    public async Task<OrdemServicoResponse> Atualizar(Guid id, AtualizarOrdemServicoRequest request)
    {
        var ordemServico = await _repository.Query().FirstOrDefaultAsync(l => l.Id == id);
        if (ordemServico is null)
        {
            throw new NotFoundException($"Ordem de serviço de id: {id} não encontrado.");
        }

        //ordemServico.Atualizar(request.Nome);

        _repository.Update(ordemServico);
        await _repository.SaveChangesAsync();

        return MapToResponse(ordemServico);
    }

    public async Task Remover(Guid id)
    {
        var ordemServico = await _repository.Query().FirstOrDefaultAsync(l => l.Id == id);
        if (ordemServico is null)
        {
            throw new NotFoundException($"Ordem de serviço de id: {id} não encontrado.");
        }

        _repository.Delete(ordemServico);
        await _repository.SaveChangesAsync();
    }
}*/