using SoatTechChallenge.Domain.OrdensServico.Produtos;
using Xunit;

namespace SoatTechChallenge.Tests.OrdensServico;

public class OrdemServicoProdutoTests
{
    // ────────────────────────────────────────────────────────────
    // Construtor
    // ────────────────────────────────────────────────────────────

    [Fact]
    public void Construtor_QuandoDadosValidos_PopulaPropriedadesCorretamente()
    {
        var idOS = Guid.NewGuid();
        var idProduto = Guid.NewGuid();

        var osp = new OrdemServicoProduto(idOS, idProduto, "Filtro de Óleo", 45m, 2m);

        Assert.NotEqual(Guid.Empty, osp.Id);
        Assert.Equal(idOS, osp.IdOrdemServico);
        Assert.Equal(idProduto, osp.IdProduto);
        Assert.Equal("Filtro de Óleo", osp.NomeProduto);
        Assert.Equal(45m, osp.ValorUnitario);
        Assert.Equal(2m, osp.Quantidade);
    }

    [Fact]
    public void Construtor_GeraIdUnico_CadaInstancia()
    {
        var p1 = new OrdemServicoProduto(Guid.NewGuid(), Guid.NewGuid(), "P1", 10m, 1m);
        var p2 = new OrdemServicoProduto(Guid.NewGuid(), Guid.NewGuid(), "P2", 10m, 1m);

        Assert.NotEqual(p1.Id, p2.Id);
    }

    // ────────────────────────────────────────────────────────────
    // Subtotal (computed)
    // ────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(10, 1, 10)]
    [InlineData(10, 3, 30)]
    [InlineData(99.99, 2, 199.98)]
    [InlineData(0, 5, 0)]
    public void Subtotal_RetornaValorUnitarioVezesQuantidade(decimal valorUnitario, decimal quantidade, decimal esperado)
    {
        var osp = new OrdemServicoProduto(Guid.NewGuid(), Guid.NewGuid(), "Produto", valorUnitario, quantidade);

        Assert.Equal(esperado, osp.Subtotal);
    }
}