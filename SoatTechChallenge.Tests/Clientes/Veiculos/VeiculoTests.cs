using SoatTechChallenge.Domain.Clientes.Veiculos;
using SoatTechChallenge.Domain.Common.Exceptions;
using Xunit;

namespace SoatTechChallenge.Tests.Clientes.Veiculos;

public class VeiculoTests
{
    private static readonly int AnoAtual = DateTime.Now.Year;

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Inserir_QuandoMarcaInvalida_LancaDomainException(string? marca)
    {
        var veiculo = new Veiculo();
        Assert.Throws<DomainException>(() =>
            veiculo.Inserir(Guid.NewGuid(), "ABC1234", marca!, "Civic", AnoAtual));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Inserir_QuandoModeloInvalido_LancaDomainException(string? modelo)
    {
        var veiculo = new Veiculo();
        Assert.Throws<DomainException>(() =>
            veiculo.Inserir(Guid.NewGuid(), "ABC1234", "Honda", modelo!, AnoAtual));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Inserir_QuandoPlacaInvalida_LancaDomainException(string? placa)
    {
        var veiculo = new Veiculo();
        Assert.Throws<DomainException>(() =>
            veiculo.Inserir(Guid.NewGuid(), placa!, "Honda", "Civic", AnoAtual));
    }

    [Theory]
    [InlineData(1885)]
    [InlineData(0)]
    [InlineData(-1)]
    public void Inserir_QuandoAnoMenorQue1886_LancaDomainException(int ano)
    {
        var veiculo = new Veiculo();
        Assert.Throws<DomainException>(() =>
            veiculo.Inserir(Guid.NewGuid(), "ABC1234", "Honda", "Civic", ano));
    }

    [Fact]
    public void Inserir_QuandoAnoMaiorQueAnoAtualMaisUm_LancaDomainException()
    {
        var veiculo = new Veiculo();
        Assert.Throws<DomainException>(() =>
            veiculo.Inserir(Guid.NewGuid(), "ABC1234", "Honda", "Civic", AnoAtual + 2));
    }

    [Theory]
    [InlineData(1886)]
    [InlineData(2000)]
    public void Inserir_QuandoAnoNoLimiteInferior_NaoLancaExcecao(int ano)
    {
        var veiculo = new Veiculo();
        var ex = Record.Exception(() =>
            veiculo.Inserir(Guid.NewGuid(), "ABC1234", "Ford", "Model T", ano));

        Assert.Null(ex);
    }

    [Fact]
    public void Inserir_QuandoAnoIgualAnoAtualMaisUm_NaoLancaExcecao()
    {
        var veiculo = new Veiculo();
        var ex = Record.Exception(() =>
            veiculo.Inserir(Guid.NewGuid(), "ABC1234", "Honda", "Civic", AnoAtual + 1));

        Assert.Null(ex);
    }

    [Fact]
    public void Inserir_QuandoDadosValidos_PopulaPropriedadesCorretamente()
    {
        var idCliente = Guid.NewGuid();
        var antes = DateTime.UtcNow;
        var veiculo = new Veiculo();

        veiculo.Inserir(idCliente, "abc1d23", "Honda", "Civic", AnoAtual);

        Assert.NotEqual(Guid.Empty, veiculo.Id);
        Assert.Equal(idCliente, veiculo.IdCliente);
        Assert.Equal("ABC1D23", veiculo.Placa);   // normalizado para maiúsculo
        Assert.Equal("Honda", veiculo.Marca);
        Assert.Equal("Civic", veiculo.Modelo);
        Assert.Equal(AnoAtual, veiculo.Ano);
        Assert.True(veiculo.DataCriacao >= antes);
        Assert.True(veiculo.DataCriacao <= DateTime.UtcNow);
    }

    [Fact]
    public void Inserir_PlacaComEspacos_NormalizaParaMaiusculoSemEspacos()
    {
        var veiculo = new Veiculo();
        veiculo.Inserir(Guid.NewGuid(), "  abc1234  ", "Honda", "Civic", AnoAtual);

        Assert.Equal("ABC1234", veiculo.Placa);
    }

    [Fact]
    public void Inserir_GeraIdUnico_CadaInstancia()
    {
        var v1 = new Veiculo();
        var v2 = new Veiculo();
        v1.Inserir(Guid.NewGuid(), "AAA1111", "Honda", "Civic", AnoAtual);
        v2.Inserir(Guid.NewGuid(), "BBB2222", "Toyota", "Corolla", AnoAtual);

        Assert.NotEqual(v1.Id, v2.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Atualizar_QuandoMarcaInvalida_LancaDomainException(string? marca)
    {
        var veiculo = VeiculoValido();
        Assert.Throws<DomainException>(() =>
            veiculo.Atualizar("ABC1234", marca!, "Civic", AnoAtual));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Atualizar_QuandoModeloInvalido_LancaDomainException(string? modelo)
    {
        var veiculo = VeiculoValido();
        Assert.Throws<DomainException>(() =>
            veiculo.Atualizar("ABC1234", "Honda", modelo!, AnoAtual));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Atualizar_QuandoPlacaInvalida_LancaDomainException(string? placa)
    {
        var veiculo = VeiculoValido();
        Assert.Throws<DomainException>(() =>
            veiculo.Atualizar(placa!, "Honda", "Civic", AnoAtual));
    }

    [Fact]
    public void Atualizar_QuandoAnoInvalido_LancaDomainException()
    {
        var veiculo = VeiculoValido();
        Assert.Throws<DomainException>(() =>
            veiculo.Atualizar("ABC1234", "Honda", "Civic", AnoAtual + 2));
    }

    [Fact]
    public void Atualizar_QuandoDadosValidos_AlteraPropriedadesEMantemIdEIdCliente()
    {
        var veiculo = VeiculoValido();
        var idOriginal = veiculo.Id;
        var idClienteOriginal = veiculo.IdCliente;

        veiculo.Atualizar("xyz9w87", "Toyota", "Corolla", AnoAtual - 1);

        Assert.Equal("XYZ9W87", veiculo.Placa);
        Assert.Equal("Toyota", veiculo.Marca);
        Assert.Equal("Corolla", veiculo.Modelo);
        Assert.Equal(AnoAtual - 1, veiculo.Ano);
        
        // Imutáveis
        Assert.Equal(idOriginal, veiculo.Id);
        Assert.Equal(idClienteOriginal, veiculo.IdCliente);
    }

    [Fact]
    public void Atualizar_PlacaComEspacos_NormalizaParaMaiusculoSemEspacos()
    {
        var veiculo = VeiculoValido();
        veiculo.Atualizar("  xyz1234  ", "Honda", "Civic", AnoAtual);

        Assert.Equal("XYZ1234", veiculo.Placa);
    }

    #region Helpers

    private static Veiculo VeiculoValido(
        string placa = "ABC1D23",
        string marca = "Honda",
        string modelo = "Civic",
        int? ano = null)
    {
        var veiculo = new Veiculo();
        veiculo.Inserir(Guid.NewGuid(), placa, marca, modelo, ano ?? AnoAtual);
        return veiculo;
    }
    
    #endregion
}