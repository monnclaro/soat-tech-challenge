using MockQueryable.Moq;
using Moq;
using SoatTechChallenge.Application.Clientes.Veiculos.DTOs;
using SoatTechChallenge.Application.Clientes.Veiculos.DTOs.Requests;
using SoatTechChallenge.Application.Clientes.Veiculos.Services;
using SoatTechChallenge.Application.Clientes.Veiculos.Services.Validators;
using SoatTechChallenge.Application.Common.DTOs;
using SoatTechChallenge.Domain.Clientes.Veiculos;
using SoatTechChallenge.Domain.Common.Exceptions;
using SoatTechChallenge.Domain.Common.Interfaces;
using Xunit;

namespace SoatTechChallenge.Tests.Clientes.Veiculos.Unit;

public class VeiculoServiceTests
{
    private readonly Mock<IRepository<Veiculo>> _repoMock = new();
    private readonly Mock<IVeiculoValidatorService> _validatorMock = new();
    private readonly VeiculoService _sut;

    private static readonly int AnoAtual = DateTime.Now.Year;

    public VeiculoServiceTests()
    {
        _sut = new VeiculoService(_repoMock.Object, _validatorMock.Object);
        // validator não lança por padrão
        _validatorMock
            .Setup(v => v.Validar(It.IsAny<Guid>(), It.IsAny<InserirVeiculoRequest>()))
            .Returns(Task.CompletedTask);
        _validatorMock
            .Setup(v => v.Validar(It.IsAny<Guid>(), It.IsAny<AtualizarVeiculoRequest>()))
            .Returns(Task.CompletedTask);
    }

    // ────────────────────────────────────────────────────────────
    // Buscar
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Buscar_QuandoVeiculoNaoExiste_LancaNotFoundException()
    {
        Setup();
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.Buscar(Guid.NewGuid()));
    }

    [Fact]
    public async Task Buscar_QuandoVeiculoExiste_RetornaResponseMapeadoCorretamente()
    {
        var veiculo = CriarVeiculo();
        Setup(veiculo);

        var result = await _sut.Buscar(veiculo.Id);

        Assert.Equal(veiculo.Id, result.Id);
        Assert.Equal(veiculo.IdCliente, result.IdCliente);
        Assert.Equal(veiculo.Placa, result.Placa);
        Assert.Equal(veiculo.Marca, result.Marca);
        Assert.Equal(veiculo.Modelo, result.Modelo);
        Assert.Equal(veiculo.Ano, result.Ano);
    }

    // ────────────────────────────────────────────────────────────
    // BuscarListaPaginada
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task BuscarListaPaginada_QuandoSemVeiculos_RetornaListaVaziaComTotalZero()
    {
        Setup();
        var result = await _sut.BuscarListaPaginada(Guid.NewGuid(), new PagedRequest(1, 10));

        Assert.Empty(result.Itens);
        Assert.Equal(0, result.Total);
    }

    [Fact]
    public async Task BuscarListaPaginada_FiltraPorIdCliente()
    {
        var idCliente = Guid.NewGuid();
        var outroCliente = Guid.NewGuid();

        var veiculosDoCliente = new[]
        {
            CriarVeiculo("ABC1234", idCliente),
            CriarVeiculo("ABC1D23", idCliente),
        };
        var veiculoOutro = CriarVeiculo("XYZ9W87", outroCliente);

        Setup(veiculosDoCliente.Append(veiculoOutro).ToArray());

        var result = await _sut.BuscarListaPaginada(idCliente, new PagedRequest(1, 10));

        Assert.Equal(2, result.Total);
        Assert.All(result.Itens, r => Assert.Equal(idCliente, r.IdCliente));
    }

    [Fact]
    public async Task BuscarListaPaginada_AplicaPaginacaoCorretamente()
    {
        var idCliente = Guid.NewGuid();
        var veiculos = Enumerable.Range(1, 5)
            .Select(i => CriarVeiculo($"AAA{i:D4}", idCliente))
            .ToArray();
        Setup(veiculos);

        var result = await _sut.BuscarListaPaginada(idCliente, new PagedRequest(Pagina: 2, Tamanho: 2));

        Assert.Equal(5, result.Total);
        Assert.Equal(2, result.Itens.Count);
        Assert.Equal(2, result.Pagina);
    }

    // ────────────────────────────────────────────────────────────
    // Inserir
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Inserir_ChamaValidatorEInsertAsync()
    {
        var idCliente = Guid.NewGuid();
        var request = RequestInserir("ABC1234");

        await _sut.Inserir(idCliente, request);

        _validatorMock.Verify(v => v.Validar(idCliente, request), Times.Once);
        _repoMock.Verify(r => r.InsertAsync(It.IsAny<Veiculo>()), Times.Once);
    }

    [Fact]
    public async Task Inserir_QuandoValidatorLanca_NaoChamaInsertAsync()
    {
        _validatorMock
            .Setup(v => v.Validar(It.IsAny<Guid>(), It.IsAny<InserirVeiculoRequest>()))
            .ThrowsAsync(new DomainException("Inválido"));

        await Assert.ThrowsAsync<DomainException>(() =>
            _sut.Inserir(Guid.NewGuid(), RequestInserir("ABC1234")));

        _repoMock.Verify(r => r.InsertAsync(It.IsAny<Veiculo>()), Times.Never);
    }

    [Fact]
    public async Task Inserir_RetornaResponseComDadosCorretos()
    {
        var idCliente = Guid.NewGuid();
        var request = RequestInserir("ABC1234");

        var result = await _sut.Inserir(idCliente, request);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(idCliente, result.IdCliente);
        Assert.Equal("ABC1234", result.Placa);
        Assert.Equal("Honda", result.Marca);
        Assert.Equal("Civic", result.Modelo);
        Assert.Equal(AnoAtual, result.Ano);
    }

    [Fact]
    public async Task Inserir_PlacaNormalizadaParaMaiusculo()
    {
        var result = await _sut.Inserir(Guid.NewGuid(), RequestInserir("abc1234"));

        // A entidade normaliza a placa para maiúsculo
        Assert.Equal("ABC1234", result.Placa);
    }

    // ────────────────────────────────────────────────────────────
    // Atualizar
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Atualizar_QuandoVeiculoNaoExiste_LancaNotFoundException()
    {
        Setup();
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.Atualizar(Guid.NewGuid(), RequestAtualizar("ABC1234")));
    }

    [Fact]
    public async Task Atualizar_ChamaValidatorComIdVeiculoCorreto()
    {
        var veiculo = CriarVeiculo();
        Setup(veiculo);
        var request = RequestAtualizar("XYZ9W87");

        await _sut.Atualizar(veiculo.Id, request);

        _validatorMock.Verify(v => v.Validar(veiculo.Id, request), Times.Once);
    }

    [Fact]
    public async Task Atualizar_QuandoValidatorLanca_NaoChamaSaveChanges()
    {
        var veiculo = CriarVeiculo();
        Setup(veiculo);
        _validatorMock
            .Setup(v => v.Validar(It.IsAny<Guid>(), It.IsAny<AtualizarVeiculoRequest>()))
            .ThrowsAsync(new DomainException("Placa duplicada"));

        await Assert.ThrowsAsync<DomainException>(() =>
            _sut.Atualizar(veiculo.Id, RequestAtualizar("ABC1234")));

        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Atualizar_QuandoVeiculoExiste_AtualizaDadosEChamaUpdate()
    {
        var veiculo = CriarVeiculo("ABC1234");
        Setup(veiculo);

        var result = await _sut.Atualizar(veiculo.Id, RequestAtualizar("XYZ9W87", "Toyota", "Corolla", AnoAtual - 1));

        Assert.Equal("XYZ9W87", result.Placa);
        Assert.Equal("Toyota", result.Marca);
        Assert.Equal("Corolla", result.Modelo);
        Assert.Equal(AnoAtual - 1, result.Ano);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Atualizar_NaoAlteraIdNemIdCliente()
    {
        var veiculo = CriarVeiculo();
        var idOriginal = veiculo.Id;
        var idClienteOriginal = veiculo.IdCliente;
        Setup(veiculo);

        var result = await _sut.Atualizar(veiculo.Id, RequestAtualizar("XYZ9W87"));

        Assert.Equal(idOriginal, result.Id);
        Assert.Equal(idClienteOriginal, result.IdCliente);
    }

    // ────────────────────────────────────────────────────────────
    // Remover
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Remover_QuandoVeiculoNaoExiste_LancaNotFoundException()
    {
        Setup();
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.Remover(Guid.NewGuid()));
    }

    [Fact]
    public async Task Remover_QuandoVeiculoExiste_ChamaDeleteAsyncComIdCorreto()
    {
        var veiculo = CriarVeiculo();
        Setup(veiculo);

        await _sut.Remover(veiculo.Id);

        _repoMock.Verify(r => r.DeleteAsync(veiculo), Times.Once);
    }

    // ────────────────────────────────────────────────────────────
    // Helpers
    // ────────────────────────────────────────────────────────────

    private void Setup(params Veiculo[] veiculos)
    {
        _repoMock.Setup(r => r.GetQueryable())
            .Returns(veiculos.ToList().AsQueryable().BuildMock());
    }

    private static Veiculo CriarVeiculo(string placa = "ABC1D23", Guid? idCliente = null)
    {
        var v = new Veiculo();
        v.Inserir(idCliente ?? Guid.NewGuid(), placa, "Honda", "Civic", AnoAtual);
        return v;
    }

    private static InserirVeiculoRequest RequestInserir(
        string placa = "ABC1234",
        string marca = "Honda",
        string modelo = "Civic",
        int? ano = null) =>
        new(placa, marca, modelo, ano ?? AnoAtual);

    private static AtualizarVeiculoRequest RequestAtualizar(
        string placa = "XYZ9W87",
        string marca = "Honda",
        string modelo = "Civic",
        int? ano = null) =>
        new(placa, marca, modelo, ano ?? AnoAtual);
}