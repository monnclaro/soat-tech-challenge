using Microsoft.EntityFrameworkCore;
using SoatTechChallenge.Domain.Clientes.Veiculos.Services.Validators;
using SoatTechChallenge.Host.Common.DTOs;
using SoatTechChallenge.Host.Common.Services;
using SoatTechChallenge.Host.Controllers.Clientes.Veiculos.DTOs;
using SoatTechChallenge.Host.Middlewares.Exceptions;
using SoatTechChallenge.Infrastructure.Interfaces;

namespace SoatTechChallenge.Domain.Clientes.Veiculos.Services;

public class ClienteVeiculoService : IClienteVeiculoService, IScopedService
{
    private readonly IRepository<ClienteVeiculo> _repository;
    private readonly IClienteVeiculoValidatorService _validatorService;

    private static ClienteVeiculoResponse MapToResponse(ClienteVeiculo c) => new(c.Id, c.IdCliente, c.Placa, c.Marca, c.Modelo, c.Ano, c.DataCriacao);
    
    public ClienteVeiculoService(IRepository<ClienteVeiculo> repository, IClienteVeiculoValidatorService validatorService)
    {
        _repository = repository;
        _validatorService = validatorService;
    }

    public async Task<ClienteVeiculoResponse> Buscar(Guid id)
    {
        var resultado = await _repository.Query().AsNoTracking().FirstOrDefaultAsync(l => l.Id == id);
        if (resultado is null)
        {
             throw new NotFoundException($"Veículo de id: {id} não encontrado.");
        }
        
        return MapToResponse(resultado);
    }

    public async Task<PagedResponse<ClienteVeiculoResponse>> BuscarListaPaginada(Guid idCLiente, PagedRequest request)
    {
        var query = _repository.Query().AsNoTracking()
            .Where(l => l.IdCliente == idCLiente)
            .OrderBy(l => l.DataCriacao)
            .Select(l => new ClienteVeiculoResponse(l.Id, l.IdCliente, l.Placa, l.Marca, l.Modelo, l.Ano, l.DataCriacao));
        
        var total = await query.CountAsync();
        var resultado = await query
            .Skip((request.Pagina - 1) * request.Tamanho)
            .Take(request.Tamanho)
            .ToListAsync();
   
        return new PagedResponse<ClienteVeiculoResponse>(resultado, total, request.Pagina, request.Tamanho);
    }

    public async Task<ClienteVeiculoResponse> Inserir(Guid idCliente, InserirClienteVeiculoRequest request)
    {      
        var veiculo = new ClienteVeiculo();
        await veiculo.Inserir(idCliente, request, _validatorService);

        await _repository.AddAsync(veiculo);
        await _repository.SaveChangesAsync(); 
            
        return MapToResponse(veiculo);
    }

    public async Task<ClienteVeiculoResponse> Atualizar(Guid id, AtualizarClienteVeiculoRequest request)
    {
        var veiculo = await _repository.Query().FirstOrDefaultAsync(l => l.Id == id);
        if (veiculo is null)
        {
            throw new NotFoundException($"Veículo de id: {id} não encontrado.");
        }

        await veiculo.Atualizar(request, _validatorService);

        _repository.Update(veiculo);
        await _repository.SaveChangesAsync(); 
        
        return MapToResponse(veiculo);
    }

    public async Task Remover(Guid id)
    {
        var veiculo = await _repository.Query().FirstOrDefaultAsync(l => l.Id == id);
        if (veiculo is null)
        {
            throw new NotFoundException($"Veículo de id: {id} não encontrado.");
        }
        
        _repository.Delete(veiculo);
        await _repository.SaveChangesAsync(); 
    }   
}