using Domain.Produtos;
using Domain.Produtos.Gateways;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;
using SoatTechChallenge.Infrastucture.Database;
using SoatTechChallenge.Infrastucture.DomainEvents;
using SoatTechChallenge.Infrastucture.Gateways.Produtos;
using Testcontainers.PostgreSql;
using Tests.Infrastructure;
using Xunit;

namespace Tests.Produtos.Integration;

public class ProdutoGatewayIntegrationTests : IntegrationTestBase
{
    protected override void RegisterServices(IServiceCollection services)
    {
        services.AddScoped<IProdutoGateway, ProdutoGateway>();
    }
    // ── BuscarPorId ──────────────────────────────────────────────

    [Fact]
    public async Task BuscarPorId_QuandoExiste_RetornaDadosCorretos()
    {
        var produto = CriarProduto("Monitor 4K", 3500m, 8);
        await SeedAsync(produto);

        using var scope = CreateScope();
        var gateway = scope.ServiceProvider.GetRequiredService<IProdutoGateway>();

        var resultado = await gateway.BuscarPorId(produto.Id, CancellationToken.None);

        Assert.NotNull(resultado);
        Assert.Equal("Monitor 4K", resultado!.Nome);
        Assert.Equal(3500m, resultado.Valor);
        Assert.Equal(8, resultado.QuantidadeEmEstoque);
    }

    [Fact]
    public async Task BuscarPorId_QuandoNaoExiste_RetornaNull()
    {
        using var scope = CreateScope();
        var gateway = scope.ServiceProvider.GetRequiredService<IProdutoGateway>();

        var resultado = await gateway.BuscarPorId(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(resultado);
    }

    // ── BuscarPaginado ───────────────────────────────────────────

    [Fact]
    public async Task BuscarPaginado_RetornaOrdenadoPorNomeEPaginado()
    {
        await SeedAsync(CriarProduto("Zebra"), CriarProduto("Abacate"), CriarProduto("Manga"));

        using var scope = CreateScope();
        var gateway = scope.ServiceProvider.GetRequiredService<IProdutoGateway>();

        var (items, total) = await gateway.BuscarPaginado(null, new PagedRequest(1, 2), CancellationToken.None);

        Assert.Equal(3, total);
        Assert.Equal(2, items.Count);
        Assert.Equal("Abacate", items[0].Nome);
        Assert.Equal("Manga", items[1].Nome);
    }

    [Fact]
    public async Task BuscarPaginado_SegundaPagina_RetornaRestantes()
    {
        await SeedAsync(CriarProduto("Abacate"), CriarProduto("Manga"), CriarProduto("Zebra"));

        using var scope = CreateScope();
        var gateway = scope.ServiceProvider.GetRequiredService<IProdutoGateway>();

        var (items, total) = await gateway.BuscarPaginado(null, new PagedRequest(2, 2), CancellationToken.None);

        Assert.Equal(3, total);
        Assert.Single(items);
        Assert.Equal("Zebra", items[0].Nome);
    }

    // ── Atualizar ────────────────────────────────────────────────

    [Fact]
    public async Task Atualizar_QuandoExiste_PersisteMudancas()
    {
        var produto = CriarProduto("Nome Antigo", 100m);
        await SeedAsync(produto);

        using var scope = CreateScope();
        var gateway   = scope.ServiceProvider.GetRequiredService<IProdutoGateway>();
        var carregado = await gateway.BuscarPorId(produto.Id, CancellationToken.None);
        carregado!.Atualizar("Nome Novo", "Nova desc", 300m);
        await gateway.Atualizar(carregado, CancellationToken.None);

        using var verifyScope = CreateScope();
        var verificado = await verifyScope.ServiceProvider
            .GetRequiredService<IProdutoGateway>()
            .BuscarPorId(produto.Id, CancellationToken.None);

        Assert.Equal("Nome Novo", verificado!.Nome);
        Assert.Equal(300m, verificado.Valor);
    }

    // ── IncrementarEstoque ───────────────────────────────────────

    [Fact]
    public async Task Atualizar_QuandoEstoqueIncrementado_PersisteMudancas()
    {
        var produto = CriarProduto(estoque: 10);
        await SeedAsync(produto);

        using var scope = CreateScope();
        var gateway   = scope.ServiceProvider.GetRequiredService<IProdutoGateway>();
        var carregado = await gateway.BuscarPorId(produto.Id, CancellationToken.None);
        carregado!.IncrementarQuantidadeEmEstoque(5);
        await gateway.Atualizar(carregado, CancellationToken.None);

        using var verifyScope = CreateScope();
        var verificado = await verifyScope.ServiceProvider
            .GetRequiredService<IProdutoGateway>()
            .BuscarPorId(produto.Id, CancellationToken.None);

        Assert.Equal(15, verificado!.QuantidadeEmEstoque);
    }

    // ── AtualizarLote ────────────────────────────────────────────

    [Fact]
    public async Task AtualizarLote_QuandoMultiplosProdutos_PersisteTodos()
    {
        var p1 = CriarProduto("P1", estoque: 10);
        var p2 = CriarProduto("P2", estoque: 20);
        await SeedAsync(p1, p2);

        using var scope = CreateScope();
        var gateway = scope.ServiceProvider.GetRequiredService<IProdutoGateway>();
        var c1 = await gateway.BuscarPorId(p1.Id, CancellationToken.None);
        var c2 = await gateway.BuscarPorId(p2.Id, CancellationToken.None);
        c1!.DecrementarQuantidadeEmEstoque(3);
        c2!.DecrementarQuantidadeEmEstoque(5);
        await gateway.AtualizarLote([c1, c2], CancellationToken.None);

        using var verifyScope = CreateScope();
        var gw = verifyScope.ServiceProvider.GetRequiredService<IProdutoGateway>();
        var v1 = await gw.BuscarPorId(p1.Id, CancellationToken.None);
        var v2 = await gw.BuscarPorId(p2.Id, CancellationToken.None);

        Assert.Equal(7, v1!.QuantidadeEmEstoque);
        Assert.Equal(15, v2!.QuantidadeEmEstoque);
    }

    // ── Remover ──────────────────────────────────────────────────

    [Fact]
    public async Task Remover_QuandoExiste_ExcluiDoBanco()
    {
        var produto = CriarProduto();
        await SeedAsync(produto);

        using var scope = CreateScope();
        var gateway   = scope.ServiceProvider.GetRequiredService<IProdutoGateway>();
        var carregado = await gateway.BuscarPorId(produto.Id, CancellationToken.None);
        await gateway.Remover(carregado!, CancellationToken.None);

        using var verifyScope = CreateScope();
        var resultado = await verifyScope.ServiceProvider
            .GetRequiredService<IProdutoGateway>()
            .BuscarPorId(produto.Id, CancellationToken.None);

        Assert.Null(resultado);
    }

    // ── Helpers ──────────────────────────────────────────────────

    private static Produto CriarProduto(
        string nome = "Produto Teste",
        decimal valor = 100m,
        int estoque = 10)
    {
        var p = new Produto();
        p.Inserir(nome, "Descrição", valor, estoque);
        return p;
    }

    private async Task SeedAsync(params Produto[] produtos)
    {
        using var scope = CreateScope();
        var gateway = scope.ServiceProvider.GetRequiredService<IProdutoGateway>();
        foreach (var p in produtos)
            await gateway.Salvar(p, CancellationToken.None);
    }
}