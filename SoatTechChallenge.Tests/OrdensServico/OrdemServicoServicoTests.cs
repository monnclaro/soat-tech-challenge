using SoatTechChallenge.Domain.Common.Exceptions;
using SoatTechChallenge.Domain.OrdensServico.Servicos;
using SoatTechChallenge.Domain.OrdensServico.Servicos.Enums;
using Xunit;

namespace SoatTechChallenge.Tests.OrdensServico;

public class OrdemServicoServicoTests
{
    // ────────────────────────────────────────────────────────────
    // Construtor
    // ────────────────────────────────────────────────────────────

    [Fact]
    public void Construtor_QuandoDadosValidos_PopulaPropriedadesCorretamente()
    {
        var idOS = Guid.NewGuid();
        var idServico = Guid.NewGuid();

        var oss = new OrdemServicoServico(idOS, idServico, "Troca de Óleo", 150m);

        Assert.NotEqual(Guid.Empty, oss.Id);
        Assert.Equal(idOS, oss.IdOrdemServico);
        Assert.Equal(idServico, oss.IdServico);
        Assert.Equal("Troca de Óleo", oss.NomeServico);
        Assert.Equal(150m, oss.Valor);
        Assert.Equal(StatusOrdemServicoServico.AguardandoExecucao, oss.Status);
        Assert.Null(oss.DataInicioExecucao);
        Assert.Null(oss.DataFinalizacaoExecucao);
        Assert.False(oss.Status == StatusOrdemServicoServico.ExecucaoFinalizada);
    }

    [Fact]
    public void Construtor_GeraIdUnico_CadaInstancia()
    {
        var s1 = new OrdemServicoServico(Guid.NewGuid(), Guid.NewGuid(), "S1", 10m);
        var s2 = new OrdemServicoServico(Guid.NewGuid(), Guid.NewGuid(), "S2", 10m);

        Assert.NotEqual(s1.Id, s2.Id);
    }

    // ────────────────────────────────────────────────────────────
    // IniciarExecucao
    // ────────────────────────────────────────────────────────────

    [Fact]
    public void IniciarExecucao_QuandoAguardandoExecucao_MudaStatusESetaDataInicio()
    {
        var oss = ServicoNovo();
        var antes = DateTime.UtcNow;

        oss.IniciarExecucao();

        Assert.Equal(StatusOrdemServicoServico.EmExecucao, oss.Status);
        Assert.NotNull(oss.DataInicioExecucao);
        Assert.True(oss.DataInicioExecucao >= antes);
        Assert.False(oss.Status == StatusOrdemServicoServico.ExecucaoFinalizada);
    }

    [Fact]
    public void IniciarExecucao_QuandoJaEmExecucao_NaoLancaExcecao()
    {
        // A entidade não bloqueia re-início (só bloqueia se já finalizado)
        var oss = ServicoNovo();
        oss.IniciarExecucao();

        var ex = Record.Exception(() => oss.IniciarExecucao());

        Assert.Null(ex);
        Assert.Equal(StatusOrdemServicoServico.EmExecucao, oss.Status);
    }

    [Fact]
    public void IniciarExecucao_QuandoJaFinalizado_LancaDomainException()
    {
        var oss = ServicoFinalizado();

        Assert.Throws<DomainException>(() => oss.IniciarExecucao());
    }

    // ────────────────────────────────────────────────────────────
    // FinalizarExecucao
    // ────────────────────────────────────────────────────────────

    [Fact]
    public void FinalizarExecucao_QuandoEmExecucao_MudaStatusSetaDataFinalizacaoEMarcaFinalizado()
    {
        var oss = ServicoNovo();
        oss.IniciarExecucao();
        var antes = DateTime.UtcNow;

        oss.FinalizarExecucao();

        Assert.Equal(StatusOrdemServicoServico.ExecucaoFinalizada, oss.Status);
        Assert.NotNull(oss.DataFinalizacaoExecucao);
        Assert.True(oss.DataFinalizacaoExecucao >= antes);
        Assert.True(oss.Status == StatusOrdemServicoServico.ExecucaoFinalizada);
    }

    [Fact]
    public void FinalizarExecucao_QuandoJaFinalizado_LancaDomainException()
    {
        var oss = ServicoFinalizado();

        Assert.Throws<DomainException>(() => oss.FinalizarExecucao());
    }

    [Fact]
    public void Finalizado_QuandoDataFinalizacaoNula_RetornaFalse()
    {
        var oss = ServicoNovo();
        Assert.False(oss.Status == StatusOrdemServicoServico.ExecucaoFinalizada);
    }

    [Fact]
    public void Finalizado_QuandoDataFinalizacaoPreenchida_RetornaTrue()
    {
        var oss = ServicoFinalizado();
        Assert.True(oss.Status == StatusOrdemServicoServico.ExecucaoFinalizada);
    }

    // ────────────────────────────────────────────────────────────
    // Helpers
    // ────────────────────────────────────────────────────────────

    private static OrdemServicoServico ServicoNovo() =>
        new(Guid.NewGuid(), Guid.NewGuid(), "Serviço Teste", 100m);

    private static OrdemServicoServico ServicoFinalizado()
    {
        var oss = ServicoNovo();
        oss.IniciarExecucao();
        oss.FinalizarExecucao();
        return oss;
    }
}