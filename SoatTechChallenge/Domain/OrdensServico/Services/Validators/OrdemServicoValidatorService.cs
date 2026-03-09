using Microsoft.EntityFrameworkCore;
using SoatTechChallenge.Domain.Clientes;
using SoatTechChallenge.Host.Common.Services;
using SoatTechChallenge.Host.Controllers.OrdensServico.DTOs.Requests;
using SoatTechChallenge.Host.Middlewares.Exceptions;
using SoatTechChallenge.Infrastructure.Interfaces;

namespace SoatTechChallenge.Domain.OrdensServico.Services.Validators;

public class OrdemServicoValidatorService : IOrdemServicoValidatorService, IScopedService
{
    private readonly IRepository<Cliente> _clienteRepository;

    public OrdemServicoValidatorService(IRepository<Cliente> clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }

    public async Task Validar(InserirOrdemServicoRequest request)
    {
        var cliente = await _clienteRepository.Query().AsNoTracking()
            .Include(l => l.Veiculos).AsSplitQuery()
            .Where(l => l.Id == request.IdCliente)
            .FirstOrDefaultAsync();
    
        if (cliente is null)
        {
            throw new DomainException($"Cliente com Id '{request.IdCliente}' não encontrado.");
        }

        if (cliente.Veiculos.All(v => v.Id != request.IdVeiculo))
        {
            throw new DomainException($"Veículo com Id '{request.IdVeiculo}' não encontrado para o cliente '{cliente.Nome}'.");
        }
    }
}
