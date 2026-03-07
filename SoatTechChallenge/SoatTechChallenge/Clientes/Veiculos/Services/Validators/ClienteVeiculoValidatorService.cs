using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SoatTechChallenge.Clientes.Controllers.Veiculos.DTOs;
using SoatTechChallenge.Domain.Exceptions;
using SoatTechChallenge.Infrastructure.Common;
using SoatTechChallenge.Infrastructure.Interfaces;

namespace SoatTechChallenge.Clientes.Veiculos.Services.Validators;

public class ClienteVeiculoValidatorService : IClienteVeiculoValidatorService, ITransientService
{
    private readonly IRepository<Cliente> _clienteRepository;
    private readonly IRepository<ClienteVeiculo> _clienteVeiculoRepository;
    
    private static readonly Regex Antiga   = new(@"^[A-Z]{3}[\s\-]?\d{4}$",        RegexOptions.IgnoreCase);
    private static readonly Regex Mercosul = new(@"^[A-Z]{3}[\s\-]?\d[A-Z]\d{2}$", RegexOptions.IgnoreCase);
    
    public ClienteVeiculoValidatorService(IRepository<Cliente> clienteRepository, IRepository<ClienteVeiculo> clienteVeiculoRepository)
    {
        _clienteRepository = clienteRepository;
        _clienteVeiculoRepository = clienteVeiculoRepository;
    }

    public async Task Validar(Guid idCliente, InserirClienteVeiculoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Marca))
        {
            throw new DomainException("O campo 'Marca' é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(request.Modelo))
        {
            throw new DomainException("O campo 'Modelo' é obrigatório.");
        }

        if (request.Ano < 1886 || request.Ano > DateTime.Now.Year + 1)
        {
            throw new DomainException($"Ano do veículo inválido. Informe um ano entre 1886 e {DateTime.Now.Year + 1}.");
        }
                    
        if (!await _clienteRepository.Query().AsNoTracking().AnyAsync(c => c.Id == idCliente))
        {
            throw new DomainException("Cliente não encontrado.");
        }
        
        if (await _clienteVeiculoRepository.Query().AsNoTracking().AnyAsync(c => c.Placa == request.Placa))
        {
            throw new DomainException("Já existe um veículo cadastrado com a placa informada.");
        }

        if (string.IsNullOrWhiteSpace(request.Placa))
        {
            throw new DomainException("A placa do veículo é obrigatória.");
        }

        var placa = request.Placa.Trim();
        if (!Antiga.IsMatch(placa) && !Mercosul.IsMatch(placa))
        {
            throw new DomainException("Placa inválida. Use o formato ABC-1234 (antiga) ou ABC-1D23 (Mercosul).");
        }
    }

    public async Task Validar(AtualizarClienteVeiculoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Marca))
        {
            throw new DomainException("O campo 'Marca' é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(request.Modelo))
        {
            throw new DomainException("O campo 'Modelo' é obrigatório.");
        }

        if (request.Ano < 1886 || request.Ano > DateTime.Now.Year + 1)
        {
            throw new DomainException($"Ano do veículo inválido. Informe um ano entre 1886 e {DateTime.Now.Year + 1}.");
        }
        
        if (await _clienteVeiculoRepository.Query().AsNoTracking().AnyAsync(c => c.Placa == request.Placa))
        {
            throw new DomainException("Já existe um veículo cadastrado com a placa informada.");
        }
        
        var placa = request.Placa.Trim();
        if (!Antiga.IsMatch(placa) && !Mercosul.IsMatch(placa))
        {
            throw new DomainException("Placa inválida. Use o formato ABC-1234 (antiga) ou ABC-1D23 (Mercosul).");
        }
    }
}
