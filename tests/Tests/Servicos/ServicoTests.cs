using Domain.Common.Exceptions;
using Domain.Servicos;
using Xunit;

namespace Tests.Servicos;

public class ServicoTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Inserir_QuandoNomeInvalido_LancaDomainException(string? nome)
    {
        var servico = new Servico();
        Assert.Throws<DomainException>(() => servico.Inserir(nome!, "desc", 100m));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void Inserir_QuandoValorNegativo_LancaDomainException(decimal valor)
    {
        var servico = new Servico();
        Assert.Throws<DomainException>(() => servico.Inserir("Nome", "desc", valor));
    }

    [Fact]
    public void Inserir_QuandoDadosValidos_PopulaPropriedadesCorretamente()
    {
        var servico = new Servico();
        servico.Inserir("Alinhamento", "Alinhamento de rodas", 150m);

        Assert.NotEqual(Guid.Empty, servico.Id);
        Assert.Equal("Alinhamento", servico.Nome);
        Assert.Equal("Alinhamento de rodas", servico.Descricao);
        Assert.Equal(150m, servico.Valor);
    }

    [Fact]
    public void Inserir_DescricaoNula_NaoLancaExcecao()
    {
        var servico = new Servico();
        var ex = Record.Exception(() => servico.Inserir("Nome", null!, 100m));
        Assert.Null(ex);
    }

    [Fact]
    public void Inserir_GeraIdUnico_CadaInstancia()
    {
        var s1 = new Servico();
        var s2 = new Servico();
        s1.Inserir("S1", "", 10m);
        s2.Inserir("S2", "", 20m);

        Assert.NotEqual(s1.Id, s2.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Atualizar_QuandoNomeInvalido_LancaDomainException(string? nome)
    {
        var servico = ServicoValido();
        Assert.Throws<DomainException>(() => servico.Atualizar(nome!, "desc", 100m));
    }

    [Theory] 
    [InlineData(-1)]
    public void Atualizar_QuandoValorNegativo_LancaDomainException(decimal valor)
    {
        var servico = ServicoValido();
        Assert.Throws<DomainException>(() => servico.Atualizar("Nome", "desc", valor));
    }

    [Fact]
    public void Atualizar_QuandoDadosValidos_AlteraNomeDescricaoEValor()
    {
        var servico = ServicoValido();
        var idOriginal = servico.Id;

        servico.Atualizar("Balanceamento", "Balanceamento completo", 200m);

        Assert.Equal("Balanceamento", servico.Nome);
        Assert.Equal("Balanceamento completo", servico.Descricao);
        Assert.Equal(200m, servico.Valor);
        Assert.Equal(idOriginal, servico.Id);
    }

    private static Servico ServicoValido(string nome = "Serviço Teste", decimal valor = 100m)
    {
        var s = new Servico();
        s.Inserir(nome, "Descrição", valor);
        return s;
    }
}