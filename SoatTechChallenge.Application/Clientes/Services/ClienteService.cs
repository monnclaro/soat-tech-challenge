using Microsoft.EntityFrameworkCore;
using SoatTechChallenge.Application.Clientes.DTOs;
using SoatTechChallenge.Application.Clientes.DTOs.Requests;
using SoatTechChallenge.Application.Clientes.DTOs.Responses;
using SoatTechChallenge.Application.Clientes.Services.Validators;
using SoatTechChallenge.Application.Common.DTOs;
using SoatTechChallenge.Application.Common.Interfaces;
using SoatTechChallenge.Domain.Clientes;
using SoatTechChallenge.Domain.Common.Exceptions;
using SoatTechChallenge.Domain.Common.Interfaces;

namespace SoatTechChallenge.Application.Clientes.Services;

public class ClienteService : IClienteService, IScopedService
{
    private readonly IRepository<Cliente> _repository;
    private readonly IClienteValidatorService _validatorService;

    private static ClienteResponse MapToResponse(Cliente c) => new(c.Id, c.Nome, c.Documento, c.DataCriacao);

    public ClienteService(IRepository<Cliente> repository, IClienteValidatorService validatorService)
    {
        _repository = repository;
        _validatorService = validatorService;
    }

    public async Task<ClienteResponse> Buscar(Guid id)
    {
        var resultado = await _repository
            .GetQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id);

        if (resultado is null)
        {
            throw new NotFoundException("Cliente não encontrado.");
        }

        return MapToResponse(resultado);
    }

    public async Task<PagedResponse<ClienteResponse>> BuscarListaPaginada(PagedRequest request)
    {
        var query = _repository
            .GetQueryable()
            .AsNoTracking()
            .OrderBy(l => l.DataCriacao)
            .Select(l => new ClienteResponse(l.Id, l.Nome, l.Documento, l.DataCriacao));

        var total = await query.CountAsync();
        var resultado = await query
            .Skip((request.Pagina - 1) * request.Tamanho)
            .Take(request.Tamanho)
            .ToListAsync();

        return new PagedResponse<ClienteResponse>(resultado, total, request.Pagina, request.Tamanho);
    }

    public async Task<ClienteResponse> Inserir(InserirClienteRequest request)
    {
        var (tipo, documento) = await _validatorService.Validar(request);

        var cliente = new Cliente();
        cliente.Inserir(request.Nome, documento, tipo);

        await _repository.InsertAsync(cliente);
        return MapToResponse(cliente);
    }

    public async Task<ClienteResponse> Atualizar(Guid id, AtualizarClienteRequest request)
    {
        var cliente = await _repository
            .GetQueryable()
            .FirstOrDefaultAsync(l => l.Id == id);

        if (cliente is null) throw new NotFoundException("Cliente não encontrado.");

        cliente.Atualizar(request.Nome);

        await _repository.UpdateAsync(cliente);
        return MapToResponse(cliente);
    }

    public async Task Remover(Guid id)
    {
        var cliente = await _repository
            .GetQueryable()
            .FirstOrDefaultAsync(l => l.Id == id);

        if (cliente is null) throw new NotFoundException("Cliente não encontrado.");

        await _repository.DeleteAsync(cliente.Id);
    }
}