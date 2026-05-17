using Domain.Clientes.Veiculos;
using Domain.Clientes.Veiculos.ValueObjects;
using SharedKernel.Exceptions;
using Xunit;

namespace Tests.Clientes.Veiculos;

public class VeiculoTests
{
    private static readonly int AnoAtual = DateTime.Now.Year;

    // ── Inserir

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Inserir_QuandoMarcaInvalida_LancaDomainException(string? marca)
    {
        var veiculo = new Veiculo();
        Assert.Throws<DomainException>(() =>
            veiculo.Inserir(Guid.NewGuid(), Placa.Criar("ABC1234"), marca!, "Civic", AnoAtual));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Inserir_QuandoModeloInvalido_LancaDomainException(string? modelo)
    {
        var veiculo = new Veiculo();
        Assert.Throws<DomainException>(() =>
            veiculo.Inserir(Guid.NewGuid(), Placa.Criar("ABC1234"), "Honda", modelo!, AnoAtual));
    }

    [Theory]
    [InlineData(1885)]
    [InlineData(0)]
    [InlineData(-1)]
    public void Inserir_QuandoAnoMenorQue1886_LancaDomainException(int ano)
    {
        var veiculo = new Veiculo();
        Assert.Throws<DomainException>(() =>
            veiculo.Inserir(Guid.NewGuid(), Placa.Criar("ABC1234"), "Honda", "Civic", ano));
    }

    [Fact]
    public void Inserir_QuandoAnoMaiorQueAnoAtualMaisUm_LancaDomainException()
    {
        var veiculo = new Veiculo();
        Assert.Throws<DomainException>(() =>
            veiculo.Inserir(Guid.NewGuid(), Placa.Criar("ABC1234"), "Honda", "Civic", AnoAtual + 2));
    }

    [Theory]
    [InlineData(1886)]
    [InlineData(2000)]
    public void Inserir_QuandoAnoNoLimiteInferior_NaoLancaExcecao(int ano)
    {
        var veiculo = new Veiculo();
        var ex = Record.Exception(() =>
            veiculo.Inserir(Guid.NewGuid(), Placa.Criar("ABC1234"), "Ford", "Model T", ano));

        Assert.Null(ex);
    }

    [Fact]
    public void Inserir_QuandoAnoIgualAnoAtualMaisUm_NaoLancaExcecao()
    {
        var veiculo = new Veiculo();
        var ex = Record.Exception(() =>
            veiculo.Inserir(Guid.NewGuid(), Placa.Criar("ABC1234"), "Honda", "Civic", AnoAtual + 1));

        Assert.Null(ex);
    }

    [Fact]
    public void Inserir_QuandoDadosValidos_PopulaPropriedadesCorretamente()
    {
        var idCliente = Guid.NewGuid();
        var antes     = DateTime.UtcNow;
        var veiculo   = new Veiculo();
        var placa     = Placa.Criar("ABC1D23");

        veiculo.Inserir(idCliente, placa, "Honda", "Civic", AnoAtual);

        Assert.NotEqual(Guid.Empty, veiculo.Id);
        Assert.Equal(idCliente, veiculo.IdCliente);
        Assert.Equal("ABC1D23", veiculo.Placa);
        Assert.Equal("Honda", veiculo.Marca);
        Assert.Equal("Civic", veiculo.Modelo);
        Assert.Equal(AnoAtual, veiculo.Ano);
        Assert.True(veiculo.DataCriacao >= antes);
        Assert.True(veiculo.DataCriacao <= DateTime.UtcNow);
    }

    [Fact]
    public void Inserir_GeraIdUnico_CadaInstancia()
    {
        var v1 = new Veiculo();
        var v2 = new Veiculo();
        v1.Inserir(Guid.NewGuid(), Placa.Criar("AAA1111"), "Honda", "Civic", AnoAtual);
        v2.Inserir(Guid.NewGuid(), Placa.Criar("BBB2222"), "Toyota", "Corolla", AnoAtual);

        Assert.NotEqual(v1.Id, v2.Id);
    }

    // ── Atualizar ────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Atualizar_QuandoMarcaInvalida_LancaDomainException(string? marca)
    {
        var veiculo = VeiculoValido();
        Assert.Throws<DomainException>(() =>
            veiculo.Atualizar(Placa.Criar("ABC1234"), marca!, "Civic", AnoAtual));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Atualizar_QuandoModeloInvalido_LancaDomainException(string? modelo)
    {
        var veiculo = VeiculoValido();
        Assert.Throws<DomainException>(() =>
            veiculo.Atualizar(Placa.Criar("ABC1234"), "Honda", modelo!, AnoAtual));
    }

    [Fact]
    public void Atualizar_QuandoAnoInvalido_LancaDomainException()
    {
        var veiculo = VeiculoValido();
        Assert.Throws<DomainException>(() =>
            veiculo.Atualizar(Placa.Criar("ABC1234"), "Honda", "Civic", AnoAtual + 2));
    }

    [Fact]
    public void Atualizar_QuandoDadosValidos_AlteraPropriedadesEMantemIdEIdCliente()
    {
        var veiculo           = VeiculoValido();
        var idOriginal        = veiculo.Id;
        var idClienteOriginal = veiculo.IdCliente;

        veiculo.Atualizar(Placa.Criar("XYZ9W87"), "Toyota", "Corolla", AnoAtual - 1);

        Assert.Equal("XYZ9W87", veiculo.Placa);
        Assert.Equal("Toyota", veiculo.Marca);
        Assert.Equal("Corolla", veiculo.Modelo);
        Assert.Equal(AnoAtual - 1, veiculo.Ano);
        Assert.Equal(idOriginal, veiculo.Id);
        Assert.Equal(idClienteOriginal, veiculo.IdCliente);
    }

    // ── Placa Value Object ───────────────────────────────────────

    [Theory]
    [InlineData("ABC1234")]   // antiga sem hífen
    [InlineData("ABC-1234")]  // antiga com hífen
    [InlineData("ABC1D23")]   // Mercosul
    public void Placa_QuandoFormatoValido_CriaCorretamente(string placa)
    {
        var resultado = Placa.Criar(placa);
        Assert.False(string.IsNullOrWhiteSpace(resultado.Valor));
        Assert.Equal(resultado.Valor, resultado.Valor.ToUpper());
    }

    [Theory]
    [InlineData("INVALIDA")]
    [InlineData("12345")]
    [InlineData("")]
    public void Placa_QuandoFormatoInvalido_LancaDomainException(string? placa)
    {
        Assert.Throws<DomainException>(() => Placa.Criar(placa!));
    }

    [Fact]
    public void Placa_NormalizaParaMaiusculoESemEspacos()
    {
        var placa = Placa.Criar("  abc1234  ");
        Assert.Equal("ABC1234", placa.Valor);
    }

    [Fact]
    public void Placa_Igualdade_QuandoMesmoValor_SaoIguais()
    {
        var p1 = Placa.Criar("ABC1234");
        var p2 = Placa.Criar("ABC1234");
        Assert.Equal(p1, p2);
    }

    // ── Helpers ──────────────────────────────────────────────────

    private static Veiculo VeiculoValido(
        string placa = "ABC1D23",
        string marca = "Honda",
        string modelo = "Civic",
        int? ano = null)
    {
        var veiculo = new Veiculo();
        veiculo.Inserir(Guid.NewGuid(), Placa.Criar(placa), marca, modelo, ano ?? AnoAtual);
        return veiculo;
    }
}