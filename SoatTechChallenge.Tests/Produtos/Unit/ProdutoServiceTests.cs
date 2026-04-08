using MockQueryable.Moq;
using Moq;
using SoatTechChallenge.Application.Common.DTOs;
using SoatTechChallenge.Application.Produtos.DTOs.Requests;
using SoatTechChallenge.Application.Produtos.Services;
using SoatTechChallenge.Domain.Common.Exceptions;
using SoatTechChallenge.Domain.Common.Interfaces;
using SoatTechChallenge.Domain.Produtos;
using Xunit;

namespace SoatTechChallenge.Tests.Produtos.Unit;

public class ProdutoServiceTests
{
    private readonly Mock<IRepository<Produto>> _repositoryMock;
    private readonly ProdutoService _sut;

    public ProdutoServiceTests()
    {
        _repositoryMock = new Mock<IRepository<Produto>>();
        _sut = new ProdutoService(_repositoryMock.Object);
    }

    [Fact]
    public async Task Buscar_QuandoProdutoNaoExiste_LancaNotFoundException()
    {
        SetupQueryable();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.Buscar(Guid.NewGuid()));
    }

    [Fact]
    public async Task Buscar_QuandoProdutoExiste_RetornaResponseCorreto()
    {
        var produto = ProdutoValido();
        SetupQueryable(produto);

        var result = await _sut.Buscar(produto.Id);

        Assert.Equal(produto.Id, result.Id);
        Assert.Equal(produto.Nome, result.Nome);
        Assert.Equal(produto.Descricao, result.Descricao);
        Assert.Equal(produto.Valor, result.Valor);
        Assert.Equal(produto.QuantidadeEmEstoque, result.QuantidadeEmEstoque);
    }

    [Fact]
    public async Task BuscarListaPaginada_QuandoSemProdutos_RetornaListaVazia()
    {
        SetupQueryable();

        var result = await _sut.BuscarListaPaginada(new PagedRequest(1, 10));

        Assert.Empty(result.Itens);
        Assert.Equal(0, result.Total);
    }

    [Fact]
    public async Task BuscarListaPaginada_RetornaTotalCorreto()
    {
        var produtos = Enumerable.Range(1, 5)
            .Select(i => ProdutoValido(nome: $"Produto {i:D2}"))
            .ToArray();
        SetupQueryable(produtos);

        var result = await _sut.BuscarListaPaginada(new PagedRequest(1, 10));

        Assert.Equal(5, result.Total);
        Assert.Equal(5, result.Itens.Count);
    }

    [Fact]
    public async Task BuscarListaPaginada_AplicaPaginacaoCorretamente()
    {
        var produtos = Enumerable.Range(1, 10)
            .Select(i => ProdutoValido(nome: $"Produto {i:D2}"))
            .ToArray();
        SetupQueryable(produtos);

        var result = await _sut.BuscarListaPaginada(new PagedRequest(Pagina: 2, Tamanho: 3));

        Assert.Equal(3, result.Itens.Count);
        Assert.Equal(10, result.Total);
        Assert.Equal(2, result.Pagina);
    }

    [Fact]
    public async Task BuscarListaPaginada_RetornaOrdenadoPorNome()
    {
        var produtos = new[]
        {
            ProdutoValido(nome: "Zebra"),
            ProdutoValido(nome: "Abacate"),
            ProdutoValido(nome: "Manga"),
        };
        SetupQueryable(produtos);

        var result = await _sut.BuscarListaPaginada(new PagedRequest(1, 10));

        var nomes = result.Itens.Select(p => p.Nome).ToList();
        Assert.Equal(new[] { "Abacate", "Manga", "Zebra" }, nomes);
    }

    [Fact]
    public async Task Inserir_QuandoDadosValidos_ChamaInsertAsyncERetornaResponse()
    {
        var request = new InserirProdutoRequest("Teclado", "Teclado Mecânico", 350m, 20m);

        var result = await _sut.Inserir(request);

        _repositoryMock.Verify(r => r.InsertAsync(It.IsAny<Produto>()), Times.Once);
        Assert.Equal("Teclado", result.Nome);
        Assert.Equal(350m, result.Valor);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task Inserir_QuandoValorInvalido_LancaDomainException()
    {
        var request = new InserirProdutoRequest("Teclado", "desc", -1m, 5m);

        await Assert.ThrowsAsync<DomainException>(() => _sut.Inserir(request));
        _repositoryMock.Verify(r => r.InsertAsync(It.IsAny<Produto>()), Times.Never);
    }
  
    [Fact]
    public async Task Atualizar_QuandoProdutoNaoExiste_LancaNotFoundException()
    {
        SetupQueryable();
        var request = new AtualizarProdutoRequest("Nome", "desc", 10m);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.Atualizar(Guid.NewGuid(), request));
    }

    [Fact]
    public async Task Atualizar_QuandoProdutoExiste_ChamaSaveChabngesAsyncERetornaResponseAtualizado()
    {
        var produto = ProdutoValido(nome: "Antigo", valor: 100m);
        SetupQueryable(produto);

        var request = new AtualizarProdutoRequest("Novo", "Nova desc", 200m);
        var result = await _sut.Atualizar(produto.Id, request);

        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        Assert.Equal("Novo", result.Nome);
        Assert.Equal(200m, result.Valor);
    }

    [Fact]
    public async Task Atualizar_QuandoValorInvalido_LancaDomainException()
    {
        var produto = ProdutoValido();
        SetupQueryable(produto);

        var request = new AtualizarProdutoRequest("Nome", "desc", 0m);

        await Assert.ThrowsAsync<DomainException>(() =>
            _sut.Atualizar(produto.Id, request));
    }

    [Fact]
    public async Task IncrementarEstoque_QuandoProdutoNaoExiste_LancaNotFoundException()
    {
        SetupQueryable();
        var request = new AtualizarQuantidadeEstoqueProdutoRequest(5m);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.IncrementarEstoque(Guid.NewGuid(), request));
    }

    [Fact]
    public async Task IncrementarEstoque_QuandoProdutoExiste_AtualizaEstoqueEChamaUpdate()
    {
        var produto = ProdutoValido(estoque: 10m);
        SetupQueryable(produto);

        var request = new AtualizarQuantidadeEstoqueProdutoRequest(5m);
        var result = await _sut.IncrementarEstoque(produto.Id, request);

        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        Assert.Equal(15m, result.QuantidadeEmEstoque);
    }

    [Fact]
    public async Task IncrementarEstoque_QuandoQuantidadeInvalida_LancaDomainException()
    {
        var produto = ProdutoValido();
        SetupQueryable(produto);

        var request = new AtualizarQuantidadeEstoqueProdutoRequest(0m);

        await Assert.ThrowsAsync<DomainException>(() =>
            _sut.IncrementarEstoque(produto.Id, request));
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Remover_QuandoProdutoNaoExiste_NaoLancaExcecaoENaoChamaDelete()
    {
        SetupQueryable();

        var exception = await Record.ExceptionAsync(() =>
            _sut.Remover(Guid.NewGuid()));

        Assert.Null(exception);
        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Produto>()), Times.Never);
    }

    [Fact]
    public async Task Remover_QuandoProdutoExiste_ChamaDeleteAsyncComIdCorreto()
    {
        var produto = ProdutoValido();
        SetupQueryable(produto);

        await _sut.Remover(produto.Id);

        _repositoryMock.Verify(r => r.DeleteAsync(produto), Times.Once);
    }

    private static Produto ProdutoValido(
        string nome = "Produto Teste",
        string descricao = "Descrição Teste",
        decimal valor = 100m,
        decimal estoque = 10m)
    {
        var produto = new Produto();
        produto.Inserir(nome, descricao, valor, estoque);
        return produto;
    }

    private void SetupQueryable(params Produto[] produtos)
    {
        var mock = produtos
            .ToList()
            .AsQueryable()
            .BuildMock();

        _repositoryMock
            .Setup(r => r.GetQueryable())
            .Returns(mock);
    }
}