using Domain.Clientes;
using Domain.Clientes.Gateways;
using Domain.Clientes.ValueObjects;
using Domain.Clientes.Veiculos;
using Domain.Clientes.Veiculos.Gateways;
using Domain.Clientes.Veiculos.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.DTOs;
using SoatTechChallenge.Infrastucture.Gateways.Clientes;

namespace Tests.Clientes.Veiculos.Integration;

public class VeiculoGatewayIntegrationTests : IntegrationTestBase
{
    protected override void RegisterServices(IServiceCollection services)
    {
        services.AddScoped<IClienteGateway, ClienteGateway>();
        services.AddScoped<IVeiculoGateway, VeiculoGateway>();
    }

    private static readonly int AnoAtual = DateTime.Now.Year;

    // ── BuscarPorId ──────────────────────────────────────────────

    [Fact]
    public async Task BuscarPorId_QuandoExiste_RetornaDadosCorretos()
    {
        var cliente = await SeedClienteAsync();
        var veiculo = CriarVeiculo("ABC1234", cliente.Id);
        await SeedVeiculoAsync(veiculo);

        using var scope = CreateScope();
        var gateway = scope.ServiceProvider.GetRequiredService<IVeiculoGateway>();
        var resultado = await gateway.BuscarPorId(veiculo.Id, CancellationToken.None);

        Assert.NotNull(resultado);
        Assert.Equal("ABC1234", resultado!.Placa);
        Assert.Equal(cliente.Id, resultado.IdCliente);
    }

    [Fact]
    public async Task BuscarPorId_QuandoNaoExiste_RetornaNull()
    {
        using var scope = CreateScope();
        var gateway = scope.ServiceProvider.GetRequiredService<IVeiculoGateway>();
        var resultado = await gateway.BuscarPorId(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(resultado);
    }

    // ── ExisteComPlaca ───────────────────────────────────────────

    [Fact]
    public async Task ExisteComPlaca_QuandoExiste_RetornaTrue()
    {
        var cliente = await SeedClienteAsync();
        await SeedVeiculoAsync(CriarVeiculo("ABC1234", cliente.Id));

        using var scope = CreateScope();
        var gateway = scope.ServiceProvider.GetRequiredService<IVeiculoGateway>();
        var resultado = await gateway.ExisteComPlaca("ABC1234", CancellationToken.None);

        Assert.True(resultado);
    }

    [Fact]
    public async Task ExisteComPlaca_QuandoNaoExiste_RetornaFalse()
    {
        using var scope = CreateScope();
        var gateway = scope.ServiceProvider.GetRequiredService<IVeiculoGateway>();
        var resultado = await gateway.ExisteComPlaca("ABC1234", CancellationToken.None);

        Assert.False(resultado);
    }

    // ── ExisteComPlacaExcetoId ───────────────────────────────────

    [Fact]
    public async Task ExisteComPlacaExcetoId_QuandoMesmoVeiculo_RetornaFalse()
    {
        var cliente = await SeedClienteAsync();
        var veiculo = CriarVeiculo("ABC1234", cliente.Id);
        await SeedVeiculoAsync(veiculo);

        using var scope = CreateScope();
        var gateway = scope.ServiceProvider.GetRequiredService<IVeiculoGateway>();
        var resultado = await gateway.ExisteComPlacaExcetoId("ABC1234", veiculo.Id, CancellationToken.None);

        Assert.False(resultado);
    }

    [Fact]
    public async Task ExisteComPlacaExcetoId_QuandoOutroVeiculo_RetornaTrue()
    {
        var cliente = await SeedClienteAsync();
        var v1 = CriarVeiculo("ABC1234", cliente.Id);
        var v2 = CriarVeiculo("XYZ9W87", cliente.Id);
        await SeedVeiculoAsync(v1, v2);

        using var scope = CreateScope();
        var gateway = scope.ServiceProvider.GetRequiredService<IVeiculoGateway>();
        var resultado = await gateway.ExisteComPlacaExcetoId("ABC1234", v2.Id, CancellationToken.None);

        Assert.True(resultado);
    }

    // ── BuscarPaginadoPorCliente ─────────────────────────────────

    [Fact]
    public async Task BuscarPaginadoPorCliente_FiltraPorCliente()
    {
        var c1 = await SeedClienteAsync("C1", "52998224725");
        var c2 = await SeedClienteAsync("C2", "11144477735");
        await SeedVeiculoAsync(CriarVeiculo("AAA1111", c1.Id), CriarVeiculo("AAA2222", c1.Id));
        await SeedVeiculoAsync(CriarVeiculo("BBB1111", c2.Id));

        using var scope = CreateScope();
        var gateway = scope.ServiceProvider.GetRequiredService<IVeiculoGateway>();

        var (items, total) =
            await gateway.BuscarPaginadoPorCliente(c1.Id, new PagedRequest(1, 10), CancellationToken.None);

        Assert.Equal(2, total);
        Assert.All(items, v => Assert.Equal(c1.Id, v.IdCliente));
    }

    [Fact]
    public async Task BuscarPaginadoPorCliente_AplicaPaginacao()
    {
        var cliente = await SeedClienteAsync();
        await SeedVeiculoAsync(
            CriarVeiculo("AAA1111", cliente.Id),
            CriarVeiculo("AAA2222", cliente.Id),
            CriarVeiculo("AAA3333", cliente.Id));

        using var scope = CreateScope();
        var gateway = scope.ServiceProvider.GetRequiredService<IVeiculoGateway>();

        var (items, total) =
            await gateway.BuscarPaginadoPorCliente(cliente.Id, new PagedRequest(2, 2), CancellationToken.None);

        Assert.Equal(3, total);
        Assert.Single(items);
    }

    // ── Atualizar ────────────────────────────────────────────────

    [Fact]
    public async Task Atualizar_QuandoExiste_PersisteMudancas()
    {
        var cliente = await SeedClienteAsync();
        var veiculo = CriarVeiculo("ABC1234", cliente.Id);
        await SeedVeiculoAsync(veiculo);

        using var scope = CreateScope();
        var gateway = scope.ServiceProvider.GetRequiredService<IVeiculoGateway>();
        var carregado = await gateway.BuscarPorId(veiculo.Id, CancellationToken.None);
        carregado!.Atualizar(Placa.Criar("XYZ9W87"), "Toyota", "Corolla", AnoAtual - 1);
        await gateway.Atualizar(carregado, CancellationToken.None);

        using var verifyScope = CreateScope();
        var verificado = await verifyScope.ServiceProvider
            .GetRequiredService<IVeiculoGateway>()
            .BuscarPorId(veiculo.Id, CancellationToken.None);

        Assert.Equal("XYZ9W87", verificado!.Placa);
        Assert.Equal("Toyota", verificado.Marca);
    }

    // ── Remover ──────────────────────────────────────────────────

    [Fact]
    public async Task Remover_QuandoExiste_ExcluiDoBanco()
    {
        var cliente = await SeedClienteAsync();
        var veiculo = CriarVeiculo("ABC1234", cliente.Id);
        await SeedVeiculoAsync(veiculo);

        using var scope = CreateScope();
        var gateway = scope.ServiceProvider.GetRequiredService<IVeiculoGateway>();
        var carregado = await gateway.BuscarPorId(veiculo.Id, CancellationToken.None);
        await gateway.Remover(carregado!, CancellationToken.None);

        using var verifyScope = CreateScope();
        var resultado = await verifyScope.ServiceProvider
            .GetRequiredService<IVeiculoGateway>()
            .BuscarPorId(veiculo.Id, CancellationToken.None);

        Assert.Null(resultado);
    }

    [Fact]
    public async Task Remover_ApósRemocao_PlacaFicaDisponivel()
    {
        var cliente = await SeedClienteAsync();
        var veiculo = CriarVeiculo("ABC1234", cliente.Id);
        await SeedVeiculoAsync(veiculo);

        using var removeScope = CreateScope();
        var gw = removeScope.ServiceProvider.GetRequiredService<IVeiculoGateway>();
        var carregado = await gw.BuscarPorId(veiculo.Id, CancellationToken.None);
        await gw.Remover(carregado!, CancellationToken.None);

        using var verifyScope = CreateScope();
        var existe = await verifyScope.ServiceProvider
            .GetRequiredService<IVeiculoGateway>()
            .ExisteComPlaca("ABC1234", CancellationToken.None);

        Assert.False(existe);
    }

    // ── Helpers ──────────────────────────────────────────────────

    private static Veiculo CriarVeiculo(string placa, Guid idCliente)
    {
        var v = new Veiculo();
        v.Inserir(idCliente, Placa.Criar(placa), "Honda", "Civic", AnoAtual);
        return v;
    }

    private async Task<Cliente> SeedClienteAsync(
        string nome = "Cliente Teste",
        string documento = "52998224725")
    {
        using var scope = CreateScope();
        var gateway     = GetService<IClienteGateway>(scope);
        var cliente     = new Cliente();
        cliente.Inserir(nome, DocumentoCliente.Criar(documento));
        await gateway.Salvar(cliente, CancellationToken.None);
        return cliente;
    }

    private async Task SeedVeiculoAsync(params Veiculo[] veiculos)
    {
        using var scope = CreateScope();
        var gateway = scope.ServiceProvider.GetRequiredService<IVeiculoGateway>();
        foreach (var v in veiculos) await gateway.Inserir(v, CancellationToken.None);
    }
}