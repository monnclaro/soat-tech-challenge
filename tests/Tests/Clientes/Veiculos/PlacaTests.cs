using Domain.Clientes.Veiculos.ValueObjects;
using Domain.Common.Exceptions;
using Xunit;

namespace Tests.Clientes.Veiculos;

public class PlacaTests
{
    // ── Criação inválida ─────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Criar_QuandoVazioOuNulo_LancaDomainException(string? placa)
    {
        Assert.Throws<DomainException>(() => Placa.Criar(placa!));
    }

    [Theory]
    [InlineData("INVALIDA")]
    [InlineData("12345")]
    [InlineData("AB12345")]
    [InlineData("ABCD123")]
    [InlineData("ABC12D3")]
    public void Criar_QuandoFormatoInvalido_LancaDomainException(string placa)
    {
        Assert.Throws<DomainException>(() => Placa.Criar(placa));
    }

    // ── Formato antigo ───────────────────────────────────────────

    [Theory]
    [InlineData("ABC1234",    "ABC1234")]
    [InlineData("ABC-1234",   "ABC1234")]
    [InlineData("abc1234",    "ABC1234")]
    [InlineData("  ABC1234  ","ABC1234")]
    public void Criar_QuandoFormatoAntigoValido_CriaNormalizado(string entrada, string esperado)
    {
        var placa = Placa.Criar(entrada);
        Assert.Equal(esperado, placa.Valor);
    }

    // ── Formato Mercosul ─────────────────────────────────────────

    [Theory]
    [InlineData("ABC1D23",    "ABC1D23")]
    [InlineData("abc1d23",    "ABC1D23")]
    [InlineData("  ABC1D23  ","ABC1D23")]
    public void Criar_QuandoFormatoMercosulValido_CriaNormalizado(string entrada, string esperado)
    {
        var placa = Placa.Criar(entrada);
        Assert.Equal(esperado, placa.Valor);
    }

    // ── Valor ────────────────────────────────────────────────────

    [Fact]
    public void Valor_RetornaPlacaNormalizada()
    {
        var placa = Placa.Criar("ABC1234");
        Assert.Equal("ABC1234", placa.Valor);
    }

    // ── Igualdade (record já implementa por valor) ───────────────

    [Fact]
    public void Igualdade_QuandoMesmoValor_SaoIguais()
    {
        var p1 = Placa.Criar("ABC1234");
        var p2 = Placa.Criar("ABC1234");

        Assert.Equal(p1, p2);
        Assert.True(p1 == p2);
        Assert.False(p1 != p2);
    }

    [Fact]
    public void Igualdade_QuandoValoresDiferentes_NaoSaoIguais()
    {
        var p1 = Placa.Criar("ABC1234");
        var p2 = Placa.Criar("XYZ9W87");

        Assert.NotEqual(p1, p2);
        Assert.False(p1 == p2);
        Assert.True(p1 != p2);
    }

    [Fact]
    public void Igualdade_ComHifenESemHifen_SaoIguais()
    {
        var comHifen = Placa.Criar("ABC-1234");
        var semHifen = Placa.Criar("ABC1234");

        Assert.Equal(comHifen, semHifen);
    }

    [Fact]
    public void GetHashCode_QuandoMesmoValor_SaoIguais()
    {
        var p1 = Placa.Criar("ABC1234");
        var p2 = Placa.Criar("ABC1234");

        Assert.Equal(p1.GetHashCode(), p2.GetHashCode());
    }
}