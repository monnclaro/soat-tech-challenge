using Application.OrdensServico.UseCases.InserirCompleta;
using Domain.Clientes;
using Domain.Clientes.Gateways;
using Domain.Clientes.ValueObjects;
using Domain.Clientes.Veiculos;
using Domain.Clientes.Veiculos.Gateways;
using Domain.Clientes.Veiculos.ValueObjects;
using Domain.OrdensServico.Gateways;
using Domain.Produtos.Gateways;
using Domain.Servicos.Gateways;
using Microsoft.Extensions.DependencyInjection;
using SoatTechChallenge.Infrastucture.Gateways.Clientes;
using SoatTechChallenge.Infrastucture.Gateways.OrdensServico;
using SoatTechChallenge.Infrastucture.Gateways.Produtos;
using SoatTechChallenge.Infrastucture.Gateways.Servicos;

namespace Tests.OrdensServico.Integration;

public class InserirOrdemServicoCompletaIntegrationTests : IntegrationTestBase
{
    protected override void RegisterServices(IServiceCollection services)
    {
        services.AddScoped<IClienteGateway, ClienteGateway>();
        services.AddScoped<IVeiculoGateway, VeiculoGateway>();
        services.AddScoped<IServicoGateway, ServicoGateway>();
        services.AddScoped<IProdutoGateway, ProdutoGateway>();
        services.AddScoped<IOrdemServicoGateway, OrdemServicoGateway>();
    }

    private const string CpfValido   = "52998224725";
    private const string PlacaValida = "ABC1234";

    #region Persistência de cliente

    [Fact]
    public async Task Executar_QuandoClienteNaoExiste_CriaClienteNoBanco()
    {
        using var scope = CreateScope();
        var (useCase, _) = CriarUseCase(scope);

        await useCase.Execute(CriarInput(), CancellationToken.None);

        var cliente = await scope.ServiceProvider
            .GetRequiredService<IClienteGateway>()
            .BuscarPorDocumento(CpfValido, CancellationToken.None);

        Assert.NotNull(cliente);
        Assert.Equal("Cliente Teste", cliente!.Nome);
    }

    [Fact]
    public async Task Executar_QuandoClienteExiste_ReusaClienteEPreserveNome()
    {
        using var seedScope = CreateScope();
        var clienteExistente = new Cliente();
        clienteExistente.Inserir("Nome Original", DocumentoCliente.Criar(CpfValido));
        await seedScope.ServiceProvider.GetRequiredService<IClienteGateway>()
            .Salvar(clienteExistente, CancellationToken.None);

        using var scope = CreateScope();
        var (useCase, _) = CriarUseCase(scope);
        await useCase.Execute(CriarInput(), CancellationToken.None);

        using var verifyScope = CreateScope();
        var cliente = await verifyScope.ServiceProvider
            .GetRequiredService<IClienteGateway>()
            .BuscarPorDocumento(CpfValido, CancellationToken.None);

        Assert.NotNull(cliente);
        Assert.Equal("Nome Original", cliente!.Nome);
    }

    #endregion

    #region Persistência de veículo

    [Fact]
    public async Task Executar_QuandoVeiculoNaoExiste_CriaVeiculoNoBanco()
    {
        using var scope = CreateScope();
        var (useCase, _) = CriarUseCase(scope);

        await useCase.Execute(CriarInput(), CancellationToken.None);

        var veiculo = await scope.ServiceProvider
            .GetRequiredService<IVeiculoGateway>()
            .BuscarPorPlaca(PlacaValida, CancellationToken.None);

        Assert.NotNull(veiculo);
        Assert.Equal("Honda", veiculo!.Marca);
    }

    [Fact]
    public async Task Executar_QuandoVeiculoExiste_ReusaVeiculoEPreservaMarca()
    {
        using var seedScope = CreateScope();
        var clienteExistente = new Cliente();
        clienteExistente.Inserir("Cliente Teste", DocumentoCliente.Criar(CpfValido));
        await seedScope.ServiceProvider.GetRequiredService<IClienteGateway>()
            .Salvar(clienteExistente, CancellationToken.None);

        var veiculoExistente = new Veiculo();
        veiculoExistente.Inserir(clienteExistente.Id, Placa.Criar(PlacaValida), "Toyota", "Corolla", DateTime.Now.Year);
        await seedScope.ServiceProvider.GetRequiredService<IVeiculoGateway>()
            .Inserir(veiculoExistente, CancellationToken.None);

        using var scope = CreateScope();
        var (useCase, _) = CriarUseCase(scope);
        await useCase.Execute(CriarInput(), CancellationToken.None);

        using var verifyScope = CreateScope();
        var veiculo = await verifyScope.ServiceProvider
            .GetRequiredService<IVeiculoGateway>()
            .BuscarPorPlaca(PlacaValida, CancellationToken.None);

        Assert.NotNull(veiculo);
        Assert.Equal("Toyota", veiculo!.Marca);
    }

    #endregion

    #region Persistência da OS

    [Fact]
    public async Task Executar_ChamaOkEPersistOSNoBanco()
    {
        using var scope = CreateScope();
        var (useCase, presenter) = CriarUseCase(scope);

        await useCase.Execute(CriarInput(), CancellationToken.None);

        Assert.True(presenter.OkChamado);
        Assert.NotEqual(Guid.Empty, presenter.IdCriado);

        var os = await scope.ServiceProvider
            .GetRequiredService<IOrdemServicoGateway>()
            .BuscarPorId(presenter.IdCriado, CancellationToken.None);

        Assert.NotNull(os);
    }

    [Fact]
    public async Task Executar_ComServicosEProdutos_PersisteTudoNaOS()
    {
        using var scope = CreateScope();
        var (useCase, presenter) = CriarUseCase(scope);

        await useCase.Execute(
            CriarInput(
                servicos: [new InserirOrdemServicoCompletaServicoInput("Alinhamento", "Desc", 200m)],
                produtos: [new InserirOrdemServicoCompletaProdutoInput("Filtro de Óleo", "Desc", 80m, 10, 2)]),
            CancellationToken.None);

        var os = await scope.ServiceProvider
            .GetRequiredService<IOrdemServicoGateway>()
            .BuscarComServicosProdutos(presenter.IdCriado, CancellationToken.None);

        Assert.NotNull(os);
        Assert.Single(os!.Servicos);
        Assert.Single(os.Produtos);
        Assert.Equal("Alinhamento",    os.Servicos[0].NomeServico);
        Assert.Equal("Filtro de Óleo", os.Produtos[0].NomeProduto);
        Assert.Equal(2,                os.Produtos[0].Quantidade);
    }

    [Fact]
    public async Task Executar_OSVinculaClienteEVeiculoCorretos()
    {
        using var scope = CreateScope();
        var (useCase, presenter) = CriarUseCase(scope);

        await useCase.Execute(CriarInput(), CancellationToken.None);

        var clienteGw = scope.ServiceProvider.GetRequiredService<IClienteGateway>();
        var veiculoGw = scope.ServiceProvider.GetRequiredService<IVeiculoGateway>();
        var ordemGw   = scope.ServiceProvider.GetRequiredService<IOrdemServicoGateway>();

        var cliente = await clienteGw.BuscarPorDocumento(CpfValido, CancellationToken.None);
        var veiculo = await veiculoGw.BuscarPorPlaca(PlacaValida, CancellationToken.None);
        var os      = await ordemGw.BuscarPorId(presenter.IdCriado, CancellationToken.None);

        Assert.Equal(cliente!.Id, os!.IdCliente);
        Assert.Equal(veiculo!.Id, os.IdVeiculo);
    }

    #endregion

    #region Helpers

    private (InserirOrdemServicoCompletaUseCase UseCase, InserirCompletaPresenterTeste Presenter)
        CriarUseCase(IServiceScope scope)
    {
        var presenter = new InserirCompletaPresenterTeste();
        var uc = new InserirOrdemServicoCompletaUseCase(
            scope.ServiceProvider.GetRequiredService<IClienteGateway>(),
            scope.ServiceProvider.GetRequiredService<IVeiculoGateway>(),
            scope.ServiceProvider.GetRequiredService<IServicoGateway>(),
            scope.ServiceProvider.GetRequiredService<IProdutoGateway>(),
            scope.ServiceProvider.GetRequiredService<IOrdemServicoGateway>(),
            presenter);
        return (uc, presenter);
    }

    private static InserirOrdemServicoCompletaInput CriarInput(
        List<InserirOrdemServicoCompletaServicoInput>? servicos = null,
        List<InserirOrdemServicoCompletaProdutoInput>? produtos = null) =>
        new(
            new InserirOrdemServicoCompletaClienteInput(
                "Cliente Teste", CpfValido,
                new InserirOrdemServicoCompletaVeiculoInput(PlacaValida, "Honda", "Civic", DateTime.Now.Year)),
            servicos ?? [],
            produtos ?? []);

    #endregion
}

#region Fakes

internal class InserirCompletaPresenterTeste : IInserirOrdemServicoCompletaOutputPort
{
    public bool OkChamado { get; private set; }
    public Guid IdCriado  { get; private set; }
    public void Ok(Guid idOrdem) { OkChamado = true; IdCriado = idOrdem; }
}

#endregion
