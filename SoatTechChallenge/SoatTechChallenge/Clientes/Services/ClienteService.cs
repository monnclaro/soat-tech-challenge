using Microsoft.EntityFrameworkCore;
using SoatTechChallenge.Application.DTOs;
using SoatTechChallenge.Clientes.Controllers.DTOs;
using SoatTechChallenge.Clientes.Enums;
using SoatTechChallenge.Clientes.Services.Validators;
using SoatTechChallenge.Domain.Exceptions;
using SoatTechChallenge.Infrastructure.Common;
using SoatTechChallenge.Infrastructure.Interfaces;

namespace SoatTechChallenge.Clientes.Services;

public class ClienteService : IClienteService, ITransientService
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
            .Query().AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id);

        if (resultado is null)
        {
             throw new NotFoundException($"Cliente de id: {id} não encontrado.");
        }
        
        return MapToResponse(resultado);
    }

    public async Task<PagedResponse<ClienteResponse>> BuscarListaPaginada(PagedRequest request)
    {
        var resultado = await _repository.Query()
            .AsNoTracking()
            .OrderBy(l => l.DataCriacao)
            .Skip((request.Pagina - 1) * request.Tamanho)
            .Take(request.Tamanho)
            .Select(l => new ClienteResponse(
                l.Id,
                l.Nome,
                l.Documento,
                l.DataCriacao
            ))
            .ToListAsync();

        return new PagedResponse<ClienteResponse>(resultado);
    }

    public async Task<ClienteResponse> Inserir(InserirClienteRequest request)
    {      
        var cliente = new Cliente();
        await cliente.Inserir(request, _validatorService);

        await _repository.AddAsync(cliente);
        await _repository.SaveChangesAsync(); 
            
        return MapToResponse(cliente);
    }

    public async Task<ClienteResponse> Atualizar(Guid id, AtualizarClienteRequest request)
    {
        var cliente = await _repository
            .Query()
            .FirstOrDefaultAsync(l => l.Id == id);

        if (cliente is null)
        {
             throw new NotFoundException($"Cliente de id: {id} não encontrado.");
        }

        cliente.Atualizar(request.Nome);

        _repository.Update(cliente);
        await _repository.SaveChangesAsync(); 
        
        return MapToResponse(cliente);
    }

    public async Task Remover(Guid id)
    {
         var cliente = await _repository
            .Query()
            .FirstOrDefaultAsync(l => l.Id == id);

        if (cliente is null)
        {
             throw new NotFoundException($"Cliente de id: {id} não encontrado.");
        }
        
        _repository.Delete(cliente);
        await _repository.SaveChangesAsync(); 
    }   
}