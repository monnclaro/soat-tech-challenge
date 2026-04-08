using DotNet.Testcontainers.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SoatTechChallenge.Application.Common.DTOs;
using SoatTechChallenge.Application.Produtos.DTOs.Commands;
using SoatTechChallenge.Application.Produtos.DTOs.Requests;
using SoatTechChallenge.Application.Produtos.Services;
using SoatTechChallenge.Domain.Common.Exceptions;
using SoatTechChallenge.Domain.Common.Interfaces;
using SoatTechChallenge.Domain.Produtos;
using SoatTechChallenge.Infrastucture.Database;
using SoatTechChallenge.Infrastucture.DomainEvents;
using SoatTechChallenge.Infrastucture.Persistence;
using Testcontainers.PostgreSql;
using Xunit;

namespace SoatTechChallenge.Tests.Produtos.Integration;

[Collection(nameof(IntegrationTestCollection))]
public class ProdutoServiceIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("soattest_produtos")
        .WithUsername("soatuser")
        .WithPassword("soatpass")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
        .Build();

    private ServiceProvider _provider = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var services = new ServiceCollection();
        
        services.AddScoped<IDomainEventsDispatcher, NoopDomainEventsDispatcher>();
        services.AddDbContext<SoatTechChallengeDbContext>(opts => opts.UseNpgsql(_postgres.GetConnectionString()));
        services.AddScoped<IRepository<Produto>, Repository<Produto>>();
        services.AddScoped<ProdutoService>();

        _provider = services.BuildServiceProvider();

        using var scope = _provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SoatTechChallengeDbContext>();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _provider.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    private ProdutoService CreateService(IServiceScope scope)
        => scope.ServiceProvider.GetRequiredService<ProdutoService>();

    private async Task<Produto> SeedProdutoAsync(
        string nome = "Produto Seed",
        string descricao = "Descrição Seed",
        decimal valor = 100m,
        decimal estoque = 10m)
    {
        using var scope = _provider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<Produto>>();

        var produto = new Produto();
        produto.Inserir(nome, descricao, valor, estoque);

        await repo.InsertAsync(produto);
        await repo.SaveChangesAsync();
        
        return produto;
    }

    [Fact]
    public async Task Buscar_QuandoProdutoNaoExiste_LancaNotFoundException()
    {
        using var scope = _provider.CreateScope();
        var service = CreateService(scope);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.Buscar(Guid.NewGuid()));
    }

    [Fact]
    public async Task Buscar_QuandoProdutoExiste_RetornaDadosCorretos()
    {
        var produto = await SeedProdutoAsync("Monitor 4K", "Monitor Ultra Wide", 3500m, 8m);

        using var scope = _provider.CreateScope();
        var result = await CreateService(scope).Buscar(produto.Id);

        Assert.Equal(produto.Id, result.Id);
        Assert.Equal("Monitor 4K", result.Nome);
        Assert.Equal(3500m, result.Valor);
        Assert.Equal(8m, result.QuantidadeEmEstoque);
    }

    [Fact]
    public async Task BuscarListaPaginada_RetornaOrdenadoPorNomeEPaginadoCorretamente()
    {
        await SeedProdutoAsync("Zebra");
        await SeedProdutoAsync("Abacate");
        await SeedProdutoAsync("Manga");

        using var scope = _provider.CreateScope();
        var result = await CreateService(scope)
            .BuscarListaPaginada(new PagedRequest(1, 2));
    
        Assert.Equal(2, result.Itens.Count);
        Assert.Equal("Abacate", result.Itens[0].Nome);
        Assert.Equal("Manga", result.Itens[1].Nome);
        Assert.Equal(3, result.Total);
    }

    [Fact]
    public async Task Inserir_QuandoDadosValidos_PersisteProdutoNoBanco()
    {
        using var scope = _provider.CreateScope();
        var service = CreateService(scope);

        var request = new InserirProdutoRequest("Mouse Gamer", "RGB 12000 DPI", 250m, 30m);
        var result = await service.Inserir(request);
     
        using var verifyScope = _provider.CreateScope();
        var buscado = await CreateService(verifyScope).Buscar(result.Id);

        Assert.Equal("Mouse Gamer", buscado.Nome);
        Assert.Equal(250m, buscado.Valor);
    }

    [Fact]
    public async Task Inserir_QuandoValorNegativo_LancaDomainException()
    {
        using var scope = _provider.CreateScope();
        var service = CreateService(scope);

        var request = new InserirProdutoRequest("Produto", "desc", -1m, 5m);

        await Assert.ThrowsAsync<DomainException>(() => service.Inserir(request));
    }

    [Fact]
    public async Task Atualizar_QuandoProdutoExiste_PersisteMudancasNoBanco()
    {
        var produto = await SeedProdutoAsync("Nome Antigo", valor: 100m);

        using var scope = _provider.CreateScope();
        var request = new AtualizarProdutoRequest("Nome Novo", "Nova desc", 199m);
        await CreateService(scope).Atualizar(produto.Id, request);

        using var verifyScope = _provider.CreateScope();
        var buscado = await CreateService(verifyScope).Buscar(produto.Id);

        Assert.Equal("Nome Novo", buscado.Nome);
        Assert.Equal(199m, buscado.Valor);
    }

    [Fact]
    public async Task Atualizar_QuandoProdutoNaoExiste_LancaNotFoundException()
    {
        using var scope = _provider.CreateScope();
        var request = new AtualizarProdutoRequest("Nome", "desc", 10m);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService(scope).Atualizar(Guid.NewGuid(), request));
    }

    [Fact]
    public async Task IncrementarEstoque_QuandoProdutoExiste_AtualizaEstoqueNoBanco()
    {
        var produto = await SeedProdutoAsync(estoque: 10m);

        using var scope = _provider.CreateScope();
        var request = new AtualizarQuantidadeEstoqueProdutoRequest(5m);
        var result = await CreateService(scope).IncrementarEstoque(produto.Id, request);

        Assert.Equal(15m, result.QuantidadeEmEstoque);
       
        using var verifyScope = _provider.CreateScope();
        var buscado = await CreateService(verifyScope).Buscar(produto.Id);
        Assert.Equal(15m, buscado.QuantidadeEmEstoque);
    }

    [Fact]
    public async Task IncrementarEstoque_QuandoQuantidadeZero_LancaDomainException()
    {
        var produto = await SeedProdutoAsync();

        using var scope = _provider.CreateScope();
        var request = new AtualizarQuantidadeEstoqueProdutoRequest(0m);

        await Assert.ThrowsAsync<DomainException>(() =>
            CreateService(scope).IncrementarEstoque(produto.Id, request));
    }
    
    [Fact]
    public async Task DecrementarEstoque_QuandoProdutoExiste_AtualizaEstoqueNoBanco()
    {
        var produto = await SeedProdutoAsync(estoque: 10m);
        using var scope = _provider.CreateScope();

        var command = new DecrementarQuantidadeEstoqueProdutosCommand()
        {
            Produtos = new List<DecrementarQuantidadeEstoqueProdutosProdutoCommand>()
            {
                new ()
                {
                    Id = produto.Id,
                    Quantidade = 5
                }
            }
        };
            
        await CreateService(scope).DecrementarEstoque(command);
      
        using var verifyScope = _provider.CreateScope();
        var buscado = await CreateService(verifyScope).Buscar(produto.Id);
        Assert.Equal(5m, buscado.QuantidadeEmEstoque);
    }
    
    [Fact]
    public async Task DecrementarEstoque_QuandoQuantidadeZero_LancaDomainException()
    {
        var produto = await SeedProdutoAsync();

        using var scope = _provider.CreateScope();
        var command = new DecrementarQuantidadeEstoqueProdutosCommand()
        {
            Produtos = new List<DecrementarQuantidadeEstoqueProdutosProdutoCommand>()
            {
                new ()
                {
                    Id = produto.Id,
                    Quantidade = -6m
                }
            }
        };

        await Assert.ThrowsAsync<DomainException>(() => CreateService(scope).DecrementarEstoque(command));
    }

    [Fact]
    public async Task Remover_QuandoProdutoExiste_ExcluiDoBanco()
    {
        var produto = await SeedProdutoAsync();

        using var scope = _provider.CreateScope();
        await CreateService(scope).Remover(produto.Id);

        using var verifyScope = _provider.CreateScope();
        await Assert.ThrowsAsync<NotFoundException>(() => CreateService(verifyScope).Buscar(produto.Id));
    }

    [Fact]
    public async Task Remover_QuandoProdutoNaoExiste_NaoLancaExcecao()
    {
        using var scope = _provider.CreateScope();
        var exception = await Record.ExceptionAsync(() => CreateService(scope).Remover(Guid.NewGuid()));
        Assert.Null(exception);
    }
}