using Domain.Common.Exceptions;
using Domain.Produtos;

namespace Tests.Produtos;

public class ProdutoTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Inserir_QuandoNomeInvalido_LancaDomainException(string? nome)
    {
        var produto = new Produto();
        Assert.Throws<DomainException>(() => produto.Inserir(nome!, "desc", 10m, 5m));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Inserir_QuandoValorNegativo_LancaDomainException(decimal valor)
    {
        var produto = new Produto();
        Assert.Throws<DomainException>(() => produto.Inserir("Nome", "desc", valor, 5m));
    }

    [Fact]
    public void Inserir_QuandoQuantidadeNegativa_LancaDomainException()
    {
        var produto = new Produto();
        Assert.Throws<DomainException>(() =>
            produto.Inserir("Nome", "desc", 10m, -1m));
    }

    [Fact]
    public void Inserir_QuandoDadosValidos_PopulaPropriedadesCorretamente()
    {
        var produto = new Produto();
        produto.Inserir("Notebook", "Notebook Gamer", 5000m, 10m);

        Assert.NotEqual(Guid.Empty, produto.Id);
        Assert.Equal("Notebook", produto.Nome);
        Assert.Equal("Notebook Gamer", produto.Descricao);
        Assert.Equal(5000m, produto.Valor);
        Assert.Equal(10m, produto.QuantidadeEmEstoque);
    }

    [Fact]
    public void Inserir_QuandoQuantidadeZero_NaoLancaExcecao()
    {
        var produto = new Produto();
        var exception = Record.Exception(() => produto.Inserir("Nome", "desc", 10m, 0m));

        Assert.Null(exception);
    }

    [Fact]
    public void Inserir_GeraIdUnico_CadaInstancia()
    {
        var p1 = new Produto();
        var p2 = new Produto();
        p1.Inserir("P1", "d", 1m, 0m);
        p2.Inserir("P2", "d", 1m, 0m);

        Assert.NotEqual(p1.Id, p2.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Atualizar_QuandoNomeInvalido_LancaDomainException(string? nome)
    {
        var produto = ProdutoValido();
        Assert.Throws<DomainException>(() => produto.Atualizar(nome!, "desc", 10m));
    }

    [Theory]
    [InlineData(-0.01)]
    public void Atualizar_QuandoValorNegativo_LancaDomainException(decimal valor)
    {
        var produto = ProdutoValido();
        Assert.Throws<DomainException>(() => produto.Atualizar("Nome", "desc", valor));
    }

    [Fact]
    public void Atualizar_QuandoDadosValidos_AlteraNome_Descricao_Valor()
    {
        var produto = ProdutoValido();
        var idOriginal = produto.Id;
        var estoqueOriginal = produto.QuantidadeEmEstoque;

        produto.Atualizar("Novo Nome", "Nova Desc", 999m);

        Assert.Equal("Novo Nome", produto.Nome);
        Assert.Equal("Nova Desc", produto.Descricao);
        Assert.Equal(999m, produto.Valor);
        // Id e estoque não devem mudar
        Assert.Equal(idOriginal, produto.Id);
        Assert.Equal(estoqueOriginal, produto.QuantidadeEmEstoque);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void IncrementarEstoque_QuandoQuantidadeInvalida_LancaDomainException(decimal qtd)
    {
        var produto = ProdutoValido();
        Assert.Throws<DomainException>(() =>
            produto.IncrementarQuantidadeEmEstoque(qtd));
    }

    [Fact]
    public void IncrementarEstoque_QuandoQuantidadeValida_SomaAoEstoqueAtual()
    {
        var produto = ProdutoValido(estoque: 10m);
        produto.IncrementarQuantidadeEmEstoque(5m);

        Assert.Equal(15m, produto.QuantidadeEmEstoque);
    }

    [Fact]
    public void IncrementarEstoque_MultiplasChamadas_AcumulaCorretamente()
    {
        var produto = ProdutoValido(estoque: 0m);
        produto.IncrementarQuantidadeEmEstoque(3m);
        produto.IncrementarQuantidadeEmEstoque(7m);

        Assert.Equal(10m, produto.QuantidadeEmEstoque);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void DecrementarEstoque_QuandoQuantidadeNegativa_LancaDomainException(decimal quantidade)
    {
        var produto = ProdutoValido(estoque: 10m);
        Assert.Throws<DomainException>(() => produto.DecrementarQuantidadeEmEstoque(quantidade));
    }

    [Fact]
    public void DecrementarEstoque_QuandoQuantidadeValida_SubtraiDoEstoqueAtual()
    {
        var produto = ProdutoValido(estoque: 10m);
        produto.DecrementarQuantidadeEmEstoque(4m);

        Assert.Equal(6m, produto.QuantidadeEmEstoque);
    }

    [Fact]
    public void DecrementarEstoque_QuandoQuantidadeIgualAoEstoque_ZeraEstoque()
    {
        var produto = ProdutoValido(estoque: 5m);
        produto.DecrementarQuantidadeEmEstoque(5m);

        Assert.Equal(0m, produto.QuantidadeEmEstoque);
    }

    [Fact]
    public void DecrementarEstoque_MultiplasChamadas_AcumulaSubtracoesCorretamente()
    {
        var produto = ProdutoValido(estoque: 20m);
        produto.DecrementarQuantidadeEmEstoque(5m);
        produto.DecrementarQuantidadeEmEstoque(3m);

        Assert.Equal(12m, produto.QuantidadeEmEstoque);
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
}