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

public class ClienteVeiculoValidatorService : IClienteVeiculoValidatorService, IScopedService
{
    private readonly IRepository<Cliente> _clienteRepository;
    private readonly IRepository<ClienteVeiculo> _clienteVeiculoRepository;

    private static readonly Regex Antiga = new(@"^[A-Z]{3}-?\d{4}$", RegexOptions.Compiled);
    private static readonly Regex Mercosul = new(@"^[A-Z]{3}\d[A-Z]\d{2}$", RegexOptions.Compiled);

    public ClienteVeiculoValidatorService(IRepository<Cliente> clienteRepository, IRepository<ClienteVeiculo> clienteVeiculoRepository)
    {
        _clienteRepository = clienteRepository;
        _clienteVeiculoRepository = clienteVeiculoRepository;
    }

    public async Task Validar(Guid idCliente, InserirClienteVeiculoRequest request)
    {
        ValidarCamposBasicos(request);

        var placa = NormalizarPlaca(request.Placa);
        ValidarFormatoPlaca(placa);

        await ValidarClienteExiste(idCliente);
        await ValidarPlacaDuplicada(placa);
    }

    public async Task Validar(Guid idVeiculo, AtualizarClienteVeiculoRequest request)
    {
        ValidarCamposBasicos(request);

        var placa = NormalizarPlaca(request.Placa);
        ValidarFormatoPlaca(placa);

        await ValidarPlacaDuplicadaAtualizacao(idVeiculo, placa);
    }

    private static void ValidarCamposBasicos(dynamic request)
    {
        if (string.IsNullOrWhiteSpace(request.Marca))
            throw new DomainException("O campo 'Marca' é obrigatório.");

        if (string.IsNullOrWhiteSpace(request.Modelo))
            throw new DomainException("O campo 'Modelo' é obrigatório.");

        if (string.IsNullOrWhiteSpace(request.Placa))
            throw new DomainException("A placa do veículo é obrigatória.");

        var anoAtual = DateTime.Now.Year;
        if (request.Ano < 1886 || request.Ano > anoAtual + 1)
        {
            throw new DomainException($"Ano do veículo inválido. Informe um ano entre 1886 e {anoAtual + 1}.");
        }
    }

    private static string NormalizarPlaca(string placa)
    {
        return placa.Trim().ToUpper().Replace("-", "");
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

        if (veiculoExistente) throw new DomainException($"Já existe outro veículo cadastrado com a placa '{placa}'.");
    }
}
