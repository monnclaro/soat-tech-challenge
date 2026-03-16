using Microsoft.EntityFrameworkCore;
using SoatTechChallenge.Application.Clientes.Veiculos.DTOs;
using SoatTechChallenge.Application.Clientes.Veiculos.Services.Validators;
using SoatTechChallenge.Application.Common.DTOs;
using SoatTechChallenge.Application.Common.Interfaces;
using SoatTechChallenge.Domain.Clientes.Veiculos;
using SoatTechChallenge.Domain.Common.Exceptions;
using SoatTechChallenge.Domain.Common.Interfaces;
using SoatTechChallenge.Host.Controllers.Clientes.Veiculos.DTOs;

namespace SoatTechChallenge.Application.Clientes.Veiculos.Services;

public class VeiculoService : IVeiculoService, IScopedService
{
    private readonly IRepository<Veiculo> _repository;
    private readonly IVeiculoValidatorService _validatorService;

    private static ClienteVeiculoResponse MapToResponse(Veiculo c) => new(c.Id, c.IdCliente, c.Placa, c.Marca, c.Modelo, c.Ano, c.DataCriacao);

    public VeiculoService(IRepository<Veiculo> repository, IVeiculoValidatorService validatorService)
    {
        _repository = repository;
        _validatorService = validatorService;
    }

    public async Task<ClienteVeiculoResponse> Buscar(Guid id)
    {
        var resultado = await _repository.GetQueryable().AsNoTracking().FirstOrDefaultAsync(l => l.Id == id);
        if (resultado is null) throw new NotFoundException("Veículo não encontrado.");

        return MapToResponse(resultado);
    }

    public async Task<PagedResponse<ClienteVeiculoResponse>> BuscarListaPaginada(Guid idCLiente, PagedRequest request)
    {
        var query = _repository.GetQueryable().AsNoTracking()
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
        await _validatorService.Validar(idCliente, request);
        
        var veiculo = new Veiculo();
        veiculo.Inserir(idCliente, request.Placa, request.Marca, request.Modelo, request.Ano);

        await _repository.InsertAsync(veiculo);

        return MapToResponse(veiculo);
    }

    public async Task<ClienteVeiculoResponse> Atualizar(Guid id, AtualizarClienteVeiculoRequest request)
    {
        await _validatorService.Validar(id, request);
        
        var veiculo = await _repository.GetQueryable().AsNoTracking().FirstOrDefaultAsync(l => l.Id == id);
        if (veiculo is null) throw new NotFoundException("Veículo não encontrado.");

        veiculo.Atualizar(request.Placa, request.Marca, request.Modelo, request.Ano);
        
        await _repository.UpdateAsync(veiculo);
        return MapToResponse(veiculo);
    }

    public async Task Remover(Guid id)
    {
        var veiculo = await _repository.GetQueryable().AsNoTracking().FirstOrDefaultAsync(l => l.Id == id);
        if (veiculo is null) throw new NotFoundException("Veículo não encontrado.");

        await _repository.DeleteAsync(veiculo.Id);
    }
}