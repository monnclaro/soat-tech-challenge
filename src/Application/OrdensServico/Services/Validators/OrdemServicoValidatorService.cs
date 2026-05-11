using Application.OrdensServico.DTOs.Requests;
using Domain.Clientes;
using Domain.Common.Exceptions;
using Domain.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.OrdensServico.Services.Validators;

public class OrdemServicoValidatorService : IOrdemServicoValidatorService, IScopedService
{
    private readonly IRepository<Cliente> _clienteRepository;

    public OrdemServicoValidatorService(IRepository<Cliente> clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }

    public async Task Validar(InserirOrdemServicoRequest request)
    {
        var cliente = await _clienteRepository
            .GetQueryable().AsNoTracking()
            .Include(l => l.Veiculos).AsSplitQuery()
            .Where(l => l.Id == request.IdCliente)
            .FirstOrDefaultAsync();

        if (cliente is null)
        {
            throw new DomainException("Cliente não encontrado.");
        }

        if (cliente.Veiculos.All(v => v.Id != request.IdVeiculo))
        {
            throw new DomainException($"Veículo não encontrado para o cliente '{cliente.Nome}'.");
        }
    }
}
