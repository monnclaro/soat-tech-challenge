using Application.OrdensServico.UseCases.InserirCompleta;
using Domain.Clientes;
using Domain.Clientes.ValueObjects;
using Domain.Clientes.Veiculos;
using Domain.Clientes.Veiculos.ValueObjects;
using Domain.Common.Exceptions;
using Tests.Fakes;

namespace Tests.OrdensServico.Unit;

public class InserirOrdemServicoCompletaUseCaseTests
{
    private const string CpfValido = "52998224725";
    private const string PlacaValida = "ABC1234";

    #region Cliente

    [Fact]
    public async Task InserirCompleta_QuandoClienteNaoExiste_CriaESalvaCliente()
    {
        var (useCase, cliente, _, _, _) = new InserirCompletaBuilder().Build();

        await useCase.Execute(CriarInput(), CancellationToken.None);

        Assert.True(cliente.SalvarFoiChamado);
    }

    [Fact]
    public async Task InserirCompleta_QuandoClienteExiste_NaoSalvaCliente()
    {
        var clienteExistente = CriarCliente();
        var (useCase, cliente, _, _, _) = new InserirCompletaBuilder()
            .ComClienteExistente(clienteExistente)
            .Build();

        await useCase.Execute(CriarInput(), CancellationToken.None);

        Assert.False(cliente.SalvarFoiChamado);
    }

    #endregion

    #region Veículo

    [Fact]
    public async Task InserirCompleta_QuandoVeiculoNaoExiste_CriaEInsereVeiculo()
    {
        var (useCase, _, veiculo, _, _) = new InserirCompletaBuilder().Build();

        await useCase.Execute(CriarInput(), CancellationToken.None);

        Assert.True(veiculo.InserirFoiChamado);
    }

    [Fact]
    public async Task InserirCompleta_QuandoVeiculoExiste_NaoInsereVeiculo()
    {
        var clienteExistente = CriarCliente();
        var veiculoExistente = CriarVeiculo(clienteExistente.Id);
        var (useCase, _, veiculo, _, _) = new InserirCompletaBuilder()
            .ComClienteExistente(clienteExistente)
            .ComVeiculoExistente(veiculoExistente)
            .Build();

        await useCase.Execute(CriarInput(), CancellationToken.None);

        Assert.False(veiculo.InserirFoiChamado);
    }

    #endregion

    #region Itens da OS

    [Fact]
    public async Task InserirCompleta_SemServicosESemProdutos_CriaOSSemItens()
    {
        var (useCase, _, _, os, _) = new InserirCompletaBuilder().Build();

        await useCase.Execute(CriarInput(), CancellationToken.None);

        Assert.Empty(os.OsSalva!.Servicos);
        Assert.Empty(os.OsSalva.Produtos);
    }

    [Fact]
    public async Task InserirCompleta_ComServicos_AdicionaServicosNaOS()
    {
        var (useCase, _, _, os, _) = new InserirCompletaBuilder().Build();

        await useCase.Execute(
            CriarInput(servicos:
            [
                new InserirOrdemServicoCompletaServicoInput("Alinhamento",   "Desc", 200m),
                new InserirOrdemServicoCompletaServicoInput("Balanceamento", "Desc", 150m)
            ]),
            CancellationToken.None);

        Assert.Equal(2, os.OsSalva!.Servicos.Count);
        Assert.Contains(os.OsSalva.Servicos, s => s.NomeServico == "Alinhamento" && s.Valor == 200m);
        Assert.Contains(os.OsSalva.Servicos, s => s.NomeServico == "Balanceamento" && s.Valor == 150m);
    }

    [Fact]
    public async Task InserirCompleta_ComProdutos_AdicionaProdutosNaOS()
    {
        var (useCase, _, _, os, _) = new InserirCompletaBuilder().Build();

        await useCase.Execute(
            CriarInput(produtos:
            [
                new InserirOrdemServicoCompletaProdutoInput("Filtro de Óleo", "Desc", 80m, 10, 2)
            ]),
            CancellationToken.None);

        Assert.Single(os.OsSalva!.Produtos);
        Assert.Equal("Filtro de Óleo", os.OsSalva.Produtos[0].NomeProduto);
        Assert.Equal(2, os.OsSalva.Produtos[0].Quantidade);
    }

    [Fact]
    public async Task InserirCompleta_ComServicosEProdutos_AdicionaAmbosNaOS()
    {
        var (useCase, _, _, os, _) = new InserirCompletaBuilder().Build();

        await useCase.Execute(
            CriarInput(
                servicos: [new InserirOrdemServicoCompletaServicoInput("Serviço", "Desc", 100m)],
                produtos: [new InserirOrdemServicoCompletaProdutoInput("Produto", "Desc", 50m, 5, 1)]),
            CancellationToken.None);

        Assert.Single(os.OsSalva!.Servicos);
        Assert.Single(os.OsSalva.Produtos);
    }

    #endregion

    #region Resultado

    [Fact]
    public async Task InserirCompleta_ChamaOkComIdDaOrdem()
    {
        var (useCase, _, _, _, presenter) = new InserirCompletaBuilder().Build();

        await useCase.Execute(CriarInput(), CancellationToken.None);

        Assert.True(presenter.OkChamado);
        Assert.NotEqual(Guid.Empty, presenter.IdCriado);
    }

    [Fact]
    public async Task InserirCompleta_SalvaOrdemServico()
    {
        var (useCase, _, _, os, _) = new InserirCompletaBuilder().Build();

        await useCase.Execute(CriarInput(), CancellationToken.None);

        Assert.True(os.SalvarFoiChamado);
        Assert.NotNull(os.OsSalva);
    }

    [Fact]
    public async Task InserirCompleta_OSVinculaClienteEVeiculoCorretos()
    {
        var clienteExistente = CriarCliente();
        var veiculoExistente = CriarVeiculo(clienteExistente.Id);
        var (useCase, _, _, os, _) = new InserirCompletaBuilder()
            .ComClienteExistente(clienteExistente)
            .ComVeiculoExistente(veiculoExistente)
            .Build();

        await useCase.Execute(CriarInput(), CancellationToken.None);

        Assert.Equal(clienteExistente.Id, os.OsSalva!.IdCliente);
        Assert.Equal(veiculoExistente.Id, os.OsSalva.IdVeiculo);
    }

    #endregion

    #region Validações de domínio

    [Theory]
    [InlineData("11111111111")]
    [InlineData("12345678901")]
    [InlineData("abc")]
    public async Task InserirCompleta_DocumentoInvalido_LancaDomainException(string documento)
    {
        var (useCase, _, _, os, _) = new InserirCompletaBuilder().Build();

        await Assert.ThrowsAsync<DomainException>(() =>
            useCase.Execute(CriarInput(documento: documento), CancellationToken.None));

        Assert.False(os.SalvarFoiChamado);
    }

    [Theory]
    [InlineData("INVALIDA")]
    [InlineData("")]
    [InlineData("12345")]
    public async Task InserirCompleta_PlacaInvalida_LancaDomainException(string placa)
    {
        var (useCase, _, _, os, _) = new InserirCompletaBuilder().Build();

        await Assert.ThrowsAsync<DomainException>(() =>
            useCase.Execute(CriarInput(placa: placa), CancellationToken.None));

        Assert.False(os.SalvarFoiChamado);
    }

    #endregion

    #region Helpers

    private static Cliente CriarCliente()
    {
        var c = new Cliente();
        c.Inserir("Cliente Teste", DocumentoCliente.Criar(CpfValido));
        return c;
    }

    private static Veiculo CriarVeiculo(Guid idCliente)
    {
        var v = new Veiculo();
        v.Inserir(idCliente, Placa.Criar(PlacaValida), "Honda", "Civic", DateTime.Now.Year);
        return v;
    }

    private static InserirOrdemServicoCompletaInput CriarInput(
        string? documento = null,
        string? placa = null,
        List<InserirOrdemServicoCompletaServicoInput>? servicos = null,
        List<InserirOrdemServicoCompletaProdutoInput>? produtos = null) =>
        new(
            new InserirOrdemServicoCompletaClienteInput(
                "Cliente Teste",
                documento ?? CpfValido,
                new InserirOrdemServicoCompletaVeiculoInput(placa ?? PlacaValida, "Honda", "Civic", DateTime.Now.Year)),
            servicos ?? [],
            produtos ?? []);

    #endregion
}

#region Builder

file class InserirCompletaBuilder
{
    private FakeClienteGateway _cliente = new();
    private FakeVeiculoGateway _veiculo = new();
    private FakeOrdemServicoGateway _os = new();
    private FakeInserirCompletaPresenter _presenter = new();

    public InserirCompletaBuilder ComClienteExistente(Cliente c)
    {
        _cliente = new FakeClienteGateway(c);
        return this;
    }

    public InserirCompletaBuilder ComVeiculoExistente(Veiculo v)
    {
        _veiculo = new FakeVeiculoGateway(v);
        return this;
    }

    public (InserirOrdemServicoCompletaUseCase UseCase,
            FakeClienteGateway Cliente,
            FakeVeiculoGateway Veiculo,
            FakeOrdemServicoGateway OS,
            FakeInserirCompletaPresenter Presenter) Build()
    {
        var uc = new InserirOrdemServicoCompletaUseCase(
            _cliente, _veiculo, new FakeServicoGateway(), new FakeProdutoGateway(), _os, _presenter);
        return (uc, _cliente, _veiculo, _os, _presenter);
    }
}

#endregion

#region Fakes

file class FakeInserirCompletaPresenter : IInserirOrdemServicoCompletaOutputPort
{
    public bool OkChamado { get; private set; }
    public Guid IdCriado { get; private set; }
    public void Ok(Guid idOrdem) { OkChamado = true; IdCriado = idOrdem; }
}

#endregion
