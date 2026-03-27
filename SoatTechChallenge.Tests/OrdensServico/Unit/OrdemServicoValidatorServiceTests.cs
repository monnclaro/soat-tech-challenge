using MockQueryable.Moq;
using Moq;
using SoatTechChallenge.Application.OrdensServico.DTOs.Requests;
using SoatTechChallenge.Application.OrdensServico.Services.Validators;
using SoatTechChallenge.Domain.Clientes;
using SoatTechChallenge.Domain.Clientes.Enums;
using SoatTechChallenge.Domain.Clientes.Veiculos;
using SoatTechChallenge.Domain.Common.Exceptions;
using SoatTechChallenge.Domain.Common.Interfaces;
using Xunit;

namespace SoatTechChallenge.Tests.OrdensServico.Unit;

public class OrdemServicoValidatorServiceTests
{
    private readonly Mock<IRepository<Cliente>> _clienteRepoMock = new();
    private readonly OrdemServicoValidatorService _sut;

    public OrdemServicoValidatorServiceTests()
    {
        _sut = new OrdemServicoValidatorService(_clienteRepoMock.Object);
    }

    // ────────────────────────────────────────────────────────────
    // Validar – cliente não encontrado
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Validar_QuandoClienteNaoExiste_LancaDomainException()
    {
        SetupClientes();
        var request = new InserirOrdemServicoRequest(Guid.NewGuid(), Guid.NewGuid(), new List<Guid>());

        await Assert.ThrowsAsync<DomainException>(() => _sut.Validar(request));
    }

    // ────────────────────────────────────────────────────────────
    // Validar – veículo não pertence ao cliente
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Validar_QuandoVeiculoNaoPertenceAoCliente_LancaDomainException()
    {
        var cliente = CriarCliente(veiculos: new List<Veiculo> { CriarVeiculo() });
        SetupClientes(cliente);

        // IdVeiculo diferente dos veículos do cliente
        var request = new InserirOrdemServicoRequest(cliente.Id, Guid.NewGuid(), new List<Guid>());

        var ex = await Assert.ThrowsAsync<DomainException>(() => _sut.Validar(request));
        Assert.Contains(cliente.Nome, ex.Message);
    }

    // ────────────────────────────────────────────────────────────
    // Validar – sucesso
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Validar_QuandoClienteEVeiculoValidos_NaoLancaExcecao()
    {
        var veiculo = CriarVeiculo();
        var cliente = CriarCliente(veiculos: new List<Veiculo> { veiculo });
        SetupClientes(cliente);

        var request = new InserirOrdemServicoRequest(cliente.Id, veiculo.Id, new List<Guid>());

        var ex = await Record.ExceptionAsync(() => _sut.Validar(request));

        Assert.Null(ex);
    }

    [Fact]
    public async Task Validar_QuandoClienteComMultiplosVeiculos_ValidaVeiculoCorreto()
    {
        var v1 = CriarVeiculo();
        var v2 = CriarVeiculo();
        var cliente = CriarCliente(veiculos: new List<Veiculo> { v1, v2 });
        SetupClientes(cliente);

        var request = new InserirOrdemServicoRequest(cliente.Id, v2.Id, new List<Guid>());

        var ex = await Record.ExceptionAsync(() => _sut.Validar(request));
        Assert.Null(ex);
    }

    // ────────────────────────────────────────────────────────────
    // Helpers
    // ────────────────────────────────────────────────────────────

    private static Cliente CriarCliente(string nome = "Cliente Teste", List<Veiculo>? veiculos = null)
    {
        var cliente = new Cliente();
        cliente.Inserir(nome, "12345678900", TipoDocumentoCliente.Cpf);
        if (veiculos is { Count: > 0 })
            cliente.Veiculos.AddRange(veiculos);
        return cliente;
    }

    private static Veiculo CriarVeiculo()
    {
        var v = new Veiculo();
        v.Inserir(Guid.NewGuid(), "ABC1D23", "Honda", "Civic", DateTime.Now.Year);
        return v;
    }

    private void SetupClientes(params Cliente[] clientes)
    {
        var mock = clientes.ToList().AsQueryable().BuildMock();
        _clienteRepoMock.Setup(r => r.GetQueryable()).Returns(mock);
    }
}