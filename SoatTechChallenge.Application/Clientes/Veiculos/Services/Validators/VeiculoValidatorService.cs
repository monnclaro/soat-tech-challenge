using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SoatTechChallenge.Application.Clientes.Veiculos.DTOs;
using SoatTechChallenge.Application.Common.Interfaces;
using SoatTechChallenge.Domain.Clientes;
using SoatTechChallenge.Domain.Clientes.Veiculos;
using SoatTechChallenge.Domain.Common.Exceptions;
using SoatTechChallenge.Domain.Common.Interfaces;
using SoatTechChallenge.Host.Controllers.Clientes.Veiculos.DTOs;

namespace SoatTechChallenge.Application.Clientes.Veiculos.Services.Validators;

public class VeiculoValidatorService : IVeiculoValidatorService, IScopedService
{
    private readonly IRepository<Cliente> _clienteRepository;
    private readonly IRepository<Veiculo> _clienteVeiculoRepository;

    private static readonly Regex Antiga = new(@"^[A-Z]{3}-?\d{4}$", RegexOptions.Compiled);
    private static readonly Regex Mercosul = new(@"^[A-Z]{3}\d[A-Z]\d{2}$", RegexOptions.Compiled);

    public VeiculoValidatorService(IRepository<Cliente> clienteRepository, IRepository<Veiculo> clienteVeiculoRepository)
    {
        _clienteRepository = clienteRepository;
        _clienteVeiculoRepository = clienteVeiculoRepository;
    }

    public async Task Validar(Guid idCliente, InserirClienteVeiculoRequest request)
    {
        var placa = NormalizarPlaca(request.Placa);
        ValidarFormatoPlaca(placa);

        await ValidarClienteExiste(idCliente);
        await ValidarPlacaDuplicada(placa);
    }

    public async Task Validar(Guid idVeiculo, AtualizarClienteVeiculoRequest request)
    {
        var placa = NormalizarPlaca(request.Placa);
        ValidarFormatoPlaca(placa);

        await ValidarPlacaDuplicadaAtualizacao(idVeiculo, placa);
    }

    private static string NormalizarPlaca(string placa)
    {
        return placa.Trim().ToUpper();
    }

    private static void ValidarFormatoPlaca(string placa)
    {
        if (!Antiga.IsMatch(placa) && !Mercosul.IsMatch(placa))
        {
            throw new DomainException("Placa inválida. Use o formato ABC1234 (antiga) ou ABC1D23 (Mercosul).");
        }
    }

    private async Task ValidarClienteExiste(Guid idCliente)
    {
        var clienteExistente = await _clienteRepository
            .GetQueryable()
            .AsNoTracking()
            .AnyAsync(c => c.Id == idCliente);

        if (!clienteExistente) throw new DomainException("Cliente não encontrado.");
    }

    private async Task ValidarPlacaDuplicada(string placa)
    {
        var veiculoExistente = await _clienteVeiculoRepository
            .GetQueryable()
            .AsNoTracking()
            .AnyAsync(c => c.Placa == placa);

        if (veiculoExistente) throw new DomainException($"Já existe um veículo cadastrado com a placa '{placa}'.");
    }

    private async Task ValidarPlacaDuplicadaAtualizacao(Guid idVeiculo, string placa)
    {
        var veiculoExistente = await _clienteVeiculoRepository
            .GetQueryable()
            .AsNoTracking()
            .AnyAsync(c => c.Placa == placa && c.Id != idVeiculo);

        if (veiculoExistente) throw new DomainException($"Já existe um veículo cadastrado com a placa '{placa}'.");
    }
}
