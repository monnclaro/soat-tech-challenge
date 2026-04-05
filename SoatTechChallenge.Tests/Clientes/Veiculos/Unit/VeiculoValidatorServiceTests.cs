using MockQueryable.Moq;
using Moq;
using SoatTechChallenge.Application.Clientes.Veiculos.DTOs;
using SoatTechChallenge.Application.Clientes.Veiculos.DTOs.Requests;
using SoatTechChallenge.Application.Clientes.Veiculos.Services.Validators;
using SoatTechChallenge.Domain.Clientes;
using SoatTechChallenge.Domain.Clientes.Enums;
using SoatTechChallenge.Domain.Clientes.Veiculos;
using SoatTechChallenge.Domain.Common.Exceptions;
using SoatTechChallenge.Domain.Common.Interfaces;
using Xunit;

namespace SoatTechChallenge.Tests.Clientes.Veiculos.Unit;

public class VeiculoValidatorServiceTests
{
    private readonly Mock<IRepository<Cliente>> _clienteRepoMock = new();
    private readonly Mock<IRepository<Veiculo>> _veiculoRepoMock = new();
    private readonly VeiculoValidatorService _sut;

    private static readonly int AnoAtual = DateTime.Now.Year;

    public VeiculoValidatorServiceTests()
    {
        _sut = new VeiculoValidatorService(_clienteRepoMock.Object, _veiculoRepoMock.Object);
        SetupClienteExiste(true);
        SetupPlacaDuplicada(false);
    }

    // ────────────────────────────────────────────────────────────
    // NormalizarPlaca + ValidarFormato — Inserir
    // ────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("ABC123")]       // 6 chars
    [InlineData("ABCD1234")]     // 8 chars
    [InlineData("1BC1234")]      // começa com número
    [InlineData("ABC12D4")]      // Mercosul inválido (posição errada)
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validar_Inserir_QuandoPlacaInvalida_LancaDomainException(string placa)
    {
        var request = RequestInserir(placa);
        await Assert.ThrowsAsync<DomainException>(() => _sut.Validar(Guid.NewGuid(), request));
    }

    [Theory]
    [InlineData("ABC1234")]      // antiga sem hífen
    [InlineData("ABC-1234")]     // antiga com hífen
    [InlineData("abc1234")]      // minúsculo → normalizado
    [InlineData("ABC1D23")]      // Mercosul
    [InlineData("abc1d23")]      // Mercosul minúsculo → normalizado
    public async Task Validar_Inserir_QuandoPlacaValida_NaoLancaExcecaoDeFormato(string placa)
    {
        var idCliente = Guid.NewGuid();
        SetupClienteExiste(true, idCliente);
        
        var request = RequestInserir(placa);
        // Não deve lançar por formato (pode lançar por cliente/duplicada — mas temos mocks ok)
        var ex = await Record.ExceptionAsync(() => _sut.Validar(idCliente, request));
        Assert.Null(ex);
    }

    // ────────────────────────────────────────────────────────────
    // ValidarClienteExiste — Inserir
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Validar_Inserir_QuandoClienteNaoExiste_LancaDomainException()
    {
        SetupClienteExiste(false);
        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            _sut.Validar(Guid.NewGuid(), RequestInserir("ABC1234")));

        Assert.Contains("Cliente", ex.Message);
    }

    [Fact]
    public async Task Validar_Inserir_QuandoClienteExiste_NaoLancaExcecao()
    {
        var idCliente = Guid.NewGuid();
        SetupClienteExiste(true, idCliente);

        var ex = await Record.ExceptionAsync(() =>
            _sut.Validar(idCliente, RequestInserir("ABC1234")));
       
        Assert.Null(ex);
    }

    // ────────────────────────────────────────────────────────────
    // ValidarPlacaDuplicada — Inserir
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Validar_Inserir_QuandoPlacaJaCadastrada_LancaDomainException()
    {
        var idCliente = Guid.NewGuid();
        SetupClienteExiste(true, idCliente);
        SetupPlacaDuplicada(true);
        
        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            _sut.Validar(idCliente, RequestInserir("ABC1234")));

        Assert.Contains("ABC1234", ex.Message);
    }

    [Fact]
    public async Task Validar_Inserir_QuandoPlacaMinuscula_NormalizaAntesDeVerificarDuplicata()
    {
        var idCliente = Guid.NewGuid();
        SetupClienteExiste(true, idCliente);
        
        // A placa minúscula deve ser normalizada para maiúsculo antes de consultar
        SetupPlacaDuplicadaPorPlaca("ABC1234");

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            _sut.Validar(idCliente, RequestInserir("abc1234")));

        Assert.Contains("ABC1234", ex.Message);
    }

    // ────────────────────────────────────────────────────────────
    // NormalizarPlaca + ValidarFormato — Atualizar
    // ────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("ABC123")]
    [InlineData("ABCD1234")]
    [InlineData("")]
    public async Task Validar_Atualizar_QuandoPlacaInvalida_LancaDomainException(string placa)
    {
        var request = RequestAtualizar(placa);
        await Assert.ThrowsAsync<DomainException>(() => _sut.Validar(Guid.NewGuid(), request));
    }

    // ────────────────────────────────────────────────────────────
    // ValidarPlacaDuplicadaAtualizacao — ignora o próprio veículo
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Validar_Atualizar_QuandoPlacaPertenceAoProprioVeiculo_NaoLancaExcecao()
    {
        var idVeiculo = Guid.NewGuid();
        // Nenhum outro veículo tem a mesma placa → sem duplicata
        SetupPlacaDuplicadaAtualizacao(idVeiculo, "ABC1234", hasDuplicata: false);

        var ex = await Record.ExceptionAsync(() =>
            _sut.Validar(idVeiculo, RequestAtualizar("ABC1234")));

        Assert.Null(ex);
    }

    [Fact]
    public async Task Validar_Atualizar_QuandoPlacaJaUsadaPorOutroVeiculo_LancaDomainException()
    {
        var idVeiculo = Guid.NewGuid();
        SetupPlacaDuplicadaAtualizacao(idVeiculo, "ABC1234", hasDuplicata: true);

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            _sut.Validar(idVeiculo, RequestAtualizar("ABC1234")));

        Assert.Contains("ABC1234", ex.Message);
    }

    // ────────────────────────────────────────────────────────────
    // Helpers — requests
    // ────────────────────────────────────────────────────────────

    private static InserirVeiculoRequest RequestInserir(string placa) =>
        new(placa, "Honda", "Civic", AnoAtual);

    private static AtualizarVeiculoRequest RequestAtualizar(string placa) =>
        new(placa, "Toyota", "Corolla", AnoAtual);

    // ────────────────────────────────────────────────────────────
    // Helpers — mocks
    // ────────────────────────────────────────────────────────────

    private void SetupClienteExiste(bool existe, Guid? id = null)
    {
        var lista = existe
            ? new[] { CriarCliente(id ?? Guid.NewGuid()) }
            : Array.Empty<Cliente>();

        _clienteRepoMock.Setup(r => r.GetQueryable())
            .Returns(lista.AsQueryable().BuildMock());
    }

    private void SetupPlacaDuplicada(bool existe, Guid? id = null)
    {
        var lista = existe
            ? new[] { CriarVeiculo("ABC1234") }
            : Array.Empty<Veiculo>();

        _veiculoRepoMock.Setup(r => r.GetQueryable())
            .Returns(lista.AsQueryable().BuildMock());
    }

    private void SetupPlacaDuplicadaPorPlaca(string placa)
    {
        var lista = new[] { CriarVeiculo(placa) };
        _veiculoRepoMock.Setup(r => r.GetQueryable())
            .Returns(lista.AsQueryable().BuildMock());
    }

    private void SetupPlacaDuplicadaAtualizacao(Guid idVeiculo, string placa, bool hasDuplicata)
    {
        // hasDuplicata: outro veículo (id diferente) com a mesma placa
        var lista = hasDuplicata
            ? new[] { CriarVeiculoComId(Guid.NewGuid(), placa) }   // id diferente → duplicata
            : Array.Empty<Veiculo>();

        _veiculoRepoMock.Setup(r => r.GetQueryable())
            .Returns(lista.AsQueryable().BuildMock());
    }

    private static Cliente CriarCliente(Guid id)
    {
        var c = new Cliente();
        c.Inserir("Cliente", "52998224725", TipoDocumentoCliente.Cpf);

        typeof(Cliente).GetProperty(nameof(Cliente.Id))!.SetValue(c, id);

        return c;
    }

    private static Veiculo CriarVeiculo(string placa)
    {
        var v = new Veiculo();
        v.Inserir(Guid.NewGuid(), placa, "Honda", "Civic", AnoAtual);
        return v;
    }

    private static Veiculo CriarVeiculoComId(Guid id, string placa)
    {
        var v = CriarVeiculo(placa);
        typeof(Veiculo).GetProperty(nameof(Veiculo.Id))!.SetValue(v, id);
        return v;
    }
}