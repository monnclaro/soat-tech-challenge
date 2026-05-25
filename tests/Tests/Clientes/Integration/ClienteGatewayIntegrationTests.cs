using Domain.Clientes;
using Domain.Clientes.Gateways;
using Domain.Clientes.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.DTOs;
using SoatTechChallenge.Infrastucture.Gateways.Clientes;

namespace Tests.Clientes.Integration;

public class ClienteGatewayIntegrationTests : IntegrationTestBase
{
    protected override void RegisterServices(IServiceCollection services)
    {
        services.AddScoped<IClienteGateway, ClienteGateway>();

    }

    private const string CpfValido1  = "52998224725";
    private const string CpfValido2  = "11144477735";
    private const string CnpjValido  = "11222333000181";


    // ── BuscarPorId ──────────────────────────────────────────────

    [Fact]
    public async Task BuscarPorId_QuandoExiste_RetornaDadosCorretos()
    {
        var cliente = CriarCliente("João Silva", CpfValido1);
        await SeedAsync(cliente);

        using var scope = CreateScope();
        var gateway   = scope.ServiceProvider.GetRequiredService<IClienteGateway>();
        var resultado = await gateway.BuscarPorId(cliente.Id, CancellationToken.None);

        Assert.NotNull(resultado);
        Assert.Equal("João Silva", resultado!.Nome);
        Assert.Equal(CpfValido1, resultado.Documento);
    }

    [Fact]
    public async Task BuscarPorId_QuandoNaoExiste_RetornaNull()
    {
        using var scope = CreateScope();
        var gateway   = scope.ServiceProvider.GetRequiredService<IClienteGateway>();
        var resultado = await gateway.BuscarPorId(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(resultado);
    }

    // ── ExisteComDocumento ───────────────────────────────────────

    [Fact]
    public async Task ExisteComDocumento_QuandoExiste_RetornaTrue()
    {
        var cliente = CriarCliente(documento: CpfValido1);
        await SeedAsync(cliente);

        using var scope = CreateScope();
        var gateway   = scope.ServiceProvider.GetRequiredService<IClienteGateway>();
        var resultado = await gateway.ExisteComDocumento(CpfValido1, CancellationToken.None);

        Assert.True(resultado);
    }

    [Fact]
    public async Task ExisteComDocumento_QuandoNaoExiste_RetornaFalse()
    {
        using var scope = CreateScope();
        var gateway   = scope.ServiceProvider.GetRequiredService<IClienteGateway>();
        var resultado = await gateway.ExisteComDocumento(CpfValido1, CancellationToken.None);

        Assert.False(resultado);
    }

    // ── BuscarPaginado ───────────────────────────────────────────

    [Fact]
    public async Task BuscarPaginado_RetornaTotalEPaginacaoCorretos()
    {
        await SeedAsync(
            CriarCliente("A", CpfValido1),
            CriarCliente("B", CpfValido2),
            CriarCliente("C", CnpjValido));

        using var scope = CreateScope();
        var gateway = scope.ServiceProvider.GetRequiredService<IClienteGateway>();

        var (items, total) = await gateway.BuscarPaginado(new PagedRequest(1, 2), CancellationToken.None);

        Assert.Equal(3, total);
        Assert.Equal(2, items.Count);
    }

    [Fact]
    public async Task BuscarPaginado_SegundaPagina_RetornaRestantes()
    {
        await SeedAsync(
            CriarCliente("A", CpfValido1),
            CriarCliente("B", CpfValido2));

        using var scope = CreateScope();
        var gateway = scope.ServiceProvider.GetRequiredService<IClienteGateway>();

        var (items, total) = await gateway.BuscarPaginado(new PagedRequest(2, 1), CancellationToken.None);

        Assert.Equal(2, total);
        Assert.Single(items);
    }

    // ── Atualizar ────────────────────────────────────────────────

    [Fact]
    public async Task Atualizar_QuandoExiste_PersisteMudancas()
    {
        var cliente = CriarCliente("Nome Antigo", CpfValido1);
        await SeedAsync(cliente);

        using var scope = CreateScope();
        var gateway   = scope.ServiceProvider.GetRequiredService<IClienteGateway>();
        var carregado = await gateway.BuscarPorId(cliente.Id, CancellationToken.None);
        carregado!.Atualizar("Nome Novo");
        await gateway.Atualizar(carregado, CancellationToken.None);

        using var verifyScope = CreateScope();
        var verificado = await verifyScope.ServiceProvider
            .GetRequiredService<IClienteGateway>()
            .BuscarPorId(cliente.Id, CancellationToken.None);

        Assert.Equal("Nome Novo", verificado!.Nome);
        Assert.Equal(CpfValido1, verificado.Documento);
    }

    // ── Remover ──────────────────────────────────────────────────

    [Fact]
    public async Task Remover_QuandoExiste_ExcluiDoBanco()
    {
        var cliente = CriarCliente(documento: CpfValido1);
        await SeedAsync(cliente);

        using var scope = CreateScope();
        var gateway   = scope.ServiceProvider.GetRequiredService<IClienteGateway>();
        var carregado = await gateway.BuscarPorId(cliente.Id, CancellationToken.None);
        await gateway.Remover(carregado!, CancellationToken.None);

        using var verifyScope = CreateScope();
        var resultado = await verifyScope.ServiceProvider
            .GetRequiredService<IClienteGateway>()
            .BuscarPorId(cliente.Id, CancellationToken.None);

        Assert.Null(resultado);
    }

    [Fact]
    public async Task Remover_PermiteReinserirMesmoDocumento()
    {
        var cliente = CriarCliente(documento: CpfValido1);
        await SeedAsync(cliente);

        using var removeScope = CreateScope();
        var gw        = removeScope.ServiceProvider.GetRequiredService<IClienteGateway>();
        var carregado = await gw.BuscarPorId(cliente.Id, CancellationToken.None);
        await gw.Remover(carregado!, CancellationToken.None);

        using var insertScope = CreateScope();
        var novoCliente = CriarCliente("Novo Dono", CpfValido1);
        var ex = await Record.ExceptionAsync(() =>
            insertScope.ServiceProvider
                .GetRequiredService<IClienteGateway>()
                .Salvar(novoCliente, CancellationToken.None));

        Assert.Null(ex);
    }

    // ── Helpers ──────────────────────────────────────────────────

    private static Cliente CriarCliente(
        string nome = "Cliente Teste",
        string documento = "52998224725")
    {
        var c = new Cliente();
        c.Inserir(nome, DocumentoCliente.Criar(documento));
        return c;
    }

    private async Task SeedAsync(params Cliente[] clientes)
    {
        using var scope = CreateScope();
        var gateway = scope.ServiceProvider.GetRequiredService<IClienteGateway>();
        foreach (var c in clientes)
            await gateway.Salvar(c, CancellationToken.None);
    }
}