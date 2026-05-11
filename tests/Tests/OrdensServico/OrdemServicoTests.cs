using Domain.Common.Exceptions;
using Domain.OrdensServico;
using Domain.OrdensServico.Enums;
using Domain.OrdensServico.Produtos;
using Domain.OrdensServico.Servicos;
using Domain.OrdensServico.Servicos.Enums;
using Xunit;

namespace Tests.OrdensServico;

public class OrdemServicoTests
{
    // ────────────────────────────────────────────────────────────
    // Inserir
    // ────────────────────────────────────────────────────────────

    [Fact]
    public void Inserir_QuandoDadosValidos_PopulaPropriedadesCorretamente()
    {
        var idCliente = Guid.NewGuid();
        var idVeiculo = Guid.NewGuid();
        var servicos = new List<OrdemServicoServico> { CriarServico(100m) };
        var antes = DateTime.UtcNow;

        var os = new OrdemServico();
        os.Inserir(idCliente, idVeiculo, servicos);

        Assert.NotEqual(Guid.Empty, os.Id);
        Assert.Equal(idCliente, os.IdCliente);
        Assert.Equal(idVeiculo, os.IdVeiculo);
        Assert.Equal(StatusOrdemServico.Recebida, os.Status);
        Assert.True(os.DataCriacao >= antes);
        Assert.Null(os.DataInicioExecucao);
        Assert.Null(os.DataFinalizacao);
        Assert.Single(os.Servicos);
        Assert.Equal(100m, os.ValorTotal);
    }

    [Fact]
    public void Inserir_CalculaValorTotalSomenteDosServicos()
    {
        var servicos = new List<OrdemServicoServico>
        {
            CriarServico(200m),
            CriarServico(300m),
        };

        var os = new OrdemServico();
        os.Inserir(Guid.NewGuid(), Guid.NewGuid(), servicos);

        Assert.Equal(500m, os.ValorTotal);
        Assert.Empty(os.Produtos);
    }

    // ────────────────────────────────────────────────────────────
    // IniciarDiagnostico
    // ────────────────────────────────────────────────────────────

    [Fact]
    public void IniciarDiagnostico_QuandoStatusRecebida_MudaParaEmDiagnostico()
    {
        var os = OSRecebida();
        os.IniciarDiagnostico();

        Assert.Equal(StatusOrdemServico.EmDiagnostico, os.Status);
    }

    [Theory]
    [InlineData(StatusOrdemServico.EmDiagnostico)]
    [InlineData(StatusOrdemServico.AguardandoAprovacao)]
    [InlineData(StatusOrdemServico.EmExecucao)]
    [InlineData(StatusOrdemServico.Finalizada)]
    [InlineData(StatusOrdemServico.Entregue)]
    public void IniciarDiagnostico_QuandoStatusDiferenteDeRecebida_LancaDomainException(StatusOrdemServico status)
    {
        var os = OSEmStatus(status);
        Assert.Throws<DomainException>(() => os.IniciarDiagnostico());
    }

    // ────────────────────────────────────────────────────────────
    // InserirProdutos
    // ────────────────────────────────────────────────────────────

    [Fact]
    public void InserirProdutos_QuandoEmDiagnostico_AdicionaProdutosERecalculaTotal()
    {
        var os = OSEmDiagnostico(valorServico: 100m);
        var produtos = new List<OrdemServicoProduto>
        {
            CriarProduto(os.Id, 50m),
            CriarProduto(os.Id, 30m),
        };

        os.InserirProdutos(produtos);

        Assert.Equal(2, os.Produtos.Count);
        Assert.Equal(180m, os.ValorTotal);
    }

    [Theory]
    [InlineData(StatusOrdemServico.Recebida)]
    [InlineData(StatusOrdemServico.AguardandoAprovacao)]
    [InlineData(StatusOrdemServico.EmExecucao)]
    [InlineData(StatusOrdemServico.Finalizada)]
    [InlineData(StatusOrdemServico.Entregue)]
    public void InserirProdutos_QuandoStatusDiferenteDeEmDiagnostico_LancaDomainException(StatusOrdemServico status)
    {
        var os = OSEmStatus(status);
        Assert.Throws<DomainException>(() =>
            os.InserirProdutos(new List<OrdemServicoProduto> { CriarProduto(os.Id, 10m) }));
    }

    // ────────────────────────────────────────────────────────────
    // InserirServicos
    // ────────────────────────────────────────────────────────────

    [Fact]
    public void InserirServicos_QuandoEmDiagnostico_AdicionaServicosERecalculaTotal()
    {
        var os = OSEmDiagnostico(valorServico: 100m);
        os.InserirServicos(new List<OrdemServicoServico> { CriarServico(os.Id, 200m) });

        Assert.Equal(2, os.Servicos.Count);
        Assert.Equal(300m, os.ValorTotal);
    }

    [Theory]
    [InlineData(StatusOrdemServico.Recebida)]
    [InlineData(StatusOrdemServico.AguardandoAprovacao)]
    [InlineData(StatusOrdemServico.EmExecucao)]
    [InlineData(StatusOrdemServico.Finalizada)]
    [InlineData(StatusOrdemServico.Entregue)]
    public void InserirServicos_QuandoStatusDiferenteDeEmDiagnostico_LancaDomainException(StatusOrdemServico status)
    {
        var os = OSEmStatus(status);
        Assert.Throws<DomainException>(() =>
            os.InserirServicos(new List<OrdemServicoServico> { CriarServico(os.Id, 10m) }));
    }

    // ────────────────────────────────────────────────────────────
    // RemoverProduto
    // ────────────────────────────────────────────────────────────

    [Fact]
    public void RemoverProduto_QuandoEmDiagnosticoEProdutoExiste_RemoveEAtualizaTotal()
    {
        var os = OSEmDiagnostico(valorServico: 100m);
        var produto = CriarProduto(os.Id, 50m);
        os.InserirProdutos(new List<OrdemServicoProduto> { produto });

        os.RemoverProduto(produto.Id);

        Assert.Empty(os.Produtos);
        Assert.Equal(100m, os.ValorTotal);
    }

    [Fact]
    public void RemoverProduto_QuandoProdutoNaoVinculado_LancaDomainException()
    {
        var os = OSEmDiagnostico();
        Assert.Throws<DomainException>(() => os.RemoverProduto(Guid.NewGuid()));
    }

    [Theory]
    [InlineData(StatusOrdemServico.Recebida)]
    [InlineData(StatusOrdemServico.AguardandoAprovacao)]
    [InlineData(StatusOrdemServico.EmExecucao)]
    [InlineData(StatusOrdemServico.Finalizada)]
    [InlineData(StatusOrdemServico.Entregue)]
    public void RemoverProduto_QuandoStatusDiferenteDeEmDiagnostico_LancaDomainException(StatusOrdemServico status)
    {
        var os = OSEmStatus(status);
        Assert.Throws<DomainException>(() => os.RemoverProduto(Guid.NewGuid()));
    }

    // ────────────────────────────────────────────────────────────
    // RemoverServico
    // ────────────────────────────────────────────────────────────

    [Fact]
    public void RemoverServico_QuandoEmDiagnosticoEServicoExiste_RemoveEAtualizaTotal()
    {
        var os = OSEmDiagnostico(valorServico: 100m);
        var novoServico = CriarServico(os.Id, 200m);
        os.InserirServicos(new List<OrdemServicoServico> { novoServico });

        os.RemoverServico(novoServico.Id);

        Assert.Single(os.Servicos);
        Assert.Equal(100m, os.ValorTotal);
    }

    [Fact]
    public void RemoverServico_QuandoServicoNaoVinculado_LancaDomainException()
    {
        var os = OSEmDiagnostico();
        Assert.Throws<DomainException>(() => os.RemoverServico(Guid.NewGuid()));
    }

    [Theory]
    [InlineData(StatusOrdemServico.Recebida)]
    [InlineData(StatusOrdemServico.AguardandoAprovacao)]
    [InlineData(StatusOrdemServico.EmExecucao)]
    [InlineData(StatusOrdemServico.Finalizada)]
    [InlineData(StatusOrdemServico.Entregue)]
    public void RemoverServico_QuandoStatusDiferenteDeEmDiagnostico_LancaDomainException(StatusOrdemServico status)
    {
        var os = OSEmStatus(status);
        Assert.Throws<DomainException>(() => os.RemoverServico(Guid.NewGuid()));
    }

    // ────────────────────────────────────────────────────────────
    // FinalizarDiagnostico / EnviarOrcamento
    // ────────────────────────────────────────────────────────────

    [Fact]
    public void FinalizarDiagnostico_QuandoEmDiagnosticoComServicos_MudaParaAguardandoAprovacao()
    {
        var os = OSEmDiagnostico();
        os.FinalizarDiagnostico();

        Assert.Equal(StatusOrdemServico.AguardandoAprovacao, os.Status);
    }

    [Fact]
    public void FinalizarDiagnostico_QuandoSemServicos_LancaDomainException()
    {
        var os = new OrdemServico();
        os.Inserir(Guid.NewGuid(), Guid.NewGuid(), new List<OrdemServicoServico>());
        os.IniciarDiagnostico();

        Assert.Throws<DomainException>(() => os.FinalizarDiagnostico());
    }

    [Fact]
    public void FinalizarDiagnostico_QuandoStatusRecebida_LancaDomainException()
    {
        var os = OSRecebida();
        Assert.Throws<DomainException>(() => os.FinalizarDiagnostico());
    }

    // ────────────────────────────────────────────────────────────
    // AprovarOrcamento
    // ────────────────────────────────────────────────────────────

    [Fact]
    public void AprovarOrcamento_QuandoAguardandoAprovacao_MudaParaEmExecucaoESetaDataInicio()
    {
        var os = OSAguardandoAprovacao();
        var antes = DateTime.UtcNow;

        os.AprovarOrcamento();

        Assert.Equal(StatusOrdemServico.EmExecucao, os.Status);
        Assert.NotNull(os.DataInicioExecucao);
        Assert.True(os.DataInicioExecucao >= antes);
    }

    [Theory]
    [InlineData(StatusOrdemServico.Recebida)]
    [InlineData(StatusOrdemServico.EmDiagnostico)]
    [InlineData(StatusOrdemServico.EmExecucao)]
    [InlineData(StatusOrdemServico.Finalizada)]
    [InlineData(StatusOrdemServico.Entregue)]
    public void AprovarOrcamento_QuandoStatusDiferenteDeAguardandoAprovacao_LancaDomainException(StatusOrdemServico status)
    {
        var os = OSEmStatus(status);
        Assert.Throws<DomainException>(() => os.AprovarOrcamento());
    }

    // ────────────────────────────────────────────────────────────
    // IniciarExecucaoServico
    // ────────────────────────────────────────────────────────────

    [Fact]
    public void IniciarExecucaoServico_QuandoEmExecucaoEServicoExiste_IniciaExecucaoDoServico()
    {
        var servico = CriarServico(100m);
        var os = OSEmExecucao(servico);

        os.IniciarExecucaoServico(servico.Id);

        Assert.Equal(StatusOrdemServicoServico.EmExecucao, servico.Status);
        Assert.NotNull(servico.DataInicioExecucao);
    }

    [Fact]
    public void IniciarExecucaoServico_QuandoServicoNaoVinculado_LancaDomainException()
    {
        var os = OSEmExecucao();
        Assert.Throws<DomainException>(() => os.IniciarExecucaoServico(Guid.NewGuid()));
    }

    [Theory]
    [InlineData(StatusOrdemServico.Recebida)]
    [InlineData(StatusOrdemServico.EmDiagnostico)]
    [InlineData(StatusOrdemServico.AguardandoAprovacao)]
    [InlineData(StatusOrdemServico.Finalizada)]
    [InlineData(StatusOrdemServico.Entregue)]
    public void IniciarExecucaoServico_QuandoStatusDiferenteDeEmExecucao_LancaDomainException(StatusOrdemServico status)
    {
        var os = OSEmStatus(status);
        Assert.Throws<DomainException>(() => os.IniciarExecucaoServico(Guid.NewGuid()));
    }

    // ────────────────────────────────────────────────────────────
    // FinalizarExecucaoServico
    // ────────────────────────────────────────────────────────────

    [Fact]
    public void FinalizarExecucaoServico_QuandoUltimoServico_FinalizaOSESetaDataFinalizacao()
    {
        var servico = CriarServico(100m);
        
        var os = OSEmExecucao(servico);
        var antes = DateTime.UtcNow;
        
        os.IniciarExecucaoServico(os.Servicos[0].Id);
        os.FinalizarExecucaoServico(os.Servicos[0].Id);
        os.IniciarExecucaoServico(os.Servicos[1].Id);
        os.FinalizarExecucaoServico(os.Servicos[1].Id);
        
        Assert.Equal(StatusOrdemServico.Finalizada, os.Status);
        Assert.NotNull(os.DataFinalizacao);
        Assert.True(os.DataFinalizacao >= antes);
    }

    [Fact]
    public void FinalizarExecucaoServico_QuandoAindaHaServicoPendente_NaoFinalizaOS()
    {
        var s1 = CriarServico(100m);
        var s2 = CriarServico(200m);
        var os = OSEmExecucao(s1, s2);

        os.IniciarExecucaoServico(s1.Id);
        os.FinalizarExecucaoServico(s1.Id);

        Assert.Equal(StatusOrdemServico.EmExecucao, os.Status);
        Assert.Null(os.DataFinalizacao);
    }

    [Fact]
    public void FinalizarExecucaoServico_QuandoServicoNaoVinculado_LancaDomainException()
    {
        var os = OSEmExecucao();
        Assert.Throws<DomainException>(() => os.FinalizarExecucaoServico(Guid.NewGuid()));
    }

    [Theory]
    [InlineData(StatusOrdemServico.Recebida)]
    [InlineData(StatusOrdemServico.EmDiagnostico)]
    [InlineData(StatusOrdemServico.AguardandoAprovacao)]
    [InlineData(StatusOrdemServico.Finalizada)]
    [InlineData(StatusOrdemServico.Entregue)]
    public void FinalizarExecucaoServico_QuandoStatusDiferenteDeEmExecucao_LancaDomainException(StatusOrdemServico status)
    {
        var os = OSEmStatus(status);
        Assert.Throws<DomainException>(() => os.FinalizarExecucaoServico(Guid.NewGuid()));
    }

    // ────────────────────────────────────────────────────────────
    // Entregar
    // ────────────────────────────────────────────────────────────

    [Fact]
    public void Entregar_QuandoFinalizada_MudaParaEntregue()
    {
        var os = OSFinalizada();
        os.Entregar();

        Assert.Equal(StatusOrdemServico.Entregue, os.Status);
    }

    [Theory]
    [InlineData(StatusOrdemServico.Recebida)]
    [InlineData(StatusOrdemServico.EmDiagnostico)]
    [InlineData(StatusOrdemServico.AguardandoAprovacao)]
    [InlineData(StatusOrdemServico.EmExecucao)]
    [InlineData(StatusOrdemServico.Entregue)]
    public void Entregar_QuandoStatusDiferenteDeFinalizada_LancaDomainException(StatusOrdemServico status)
    {
        var os = OSEmStatus(status);
        Assert.Throws<DomainException>(() => os.Entregar());
    }

    // ────────────────────────────────────────────────────────────
    // IdsProdutos / IdsServicos (computed)
    // ────────────────────────────────────────────────────────────

    [Fact]
    public void IdsProdutos_RetornaIdsProdutoDosProdutosVinculados()
    {
        var os = OSEmDiagnostico();
        var p1 = CriarProduto(os.Id, 10m);
        var p2 = CriarProduto(os.Id, 20m);
        os.InserirProdutos(new List<OrdemServicoProduto> { p1, p2 });

        Assert.Contains(p1.IdProduto, os.IdsProdutos);
        Assert.Contains(p2.IdProduto, os.IdsProdutos);
        Assert.Equal(2, os.IdsProdutos.Count);
    }

    [Fact]
    public void IdsServicos_RetornaIdServicoDosSericosVinculados()
    {
        var s1 = CriarServico(100m);
        var s2 = CriarServico(200m);
        var os = new OrdemServico();
        os.Inserir(Guid.NewGuid(), Guid.NewGuid(), new List<OrdemServicoServico> { s1, s2 });

        Assert.Contains(s1.IdServico, os.IdsServicos);
        Assert.Contains(s2.IdServico, os.IdsServicos);
        Assert.Equal(2, os.IdsServicos.Count);
    }

    // ────────────────────────────────────────────────────────────
    // Fluxo completo (smoke test)
    // ────────────────────────────────────────────────────────────

    [Fact]
    public void FluxoCompleto_DeRecebidaAteEntregue_SemExcecoes()
    {
        var servico = CriarServico(500m);
        var os = new OrdemServico();
        os.Inserir(Guid.NewGuid(), Guid.NewGuid(), new List<OrdemServicoServico> { servico });

        os.IniciarDiagnostico();
        os.InserirProdutos(new List<OrdemServicoProduto> { CriarProduto(os.Id, 100m) });
        os.FinalizarDiagnostico();
        os.AprovarOrcamento();
        os.IniciarExecucaoServico(servico.Id);
        os.FinalizarExecucaoServico(servico.Id);
        os.Entregar();

        Assert.Equal(StatusOrdemServico.Entregue, os.Status);
        Assert.Equal(600m, os.ValorTotal);
        Assert.NotNull(os.DataInicioExecucao);
        Assert.NotNull(os.DataFinalizacao);
    }

    // ────────────────────────────────────────────────────────────
    // Helpers / factories
    // ────────────────────────────────────────────────────────────

    // Sem vínculo de OS — para uso direto em Inserir principal
    private static OrdemServicoServico CriarServico(decimal valor) =>
        new(Guid.NewGuid(), Guid.NewGuid(), "Serviço Teste", valor);

    // Com vínculo de OS explícito — para InserirServicos depois
    private static OrdemServicoServico CriarServico(Guid idOs, decimal valor) =>
        new(idOs, Guid.NewGuid(), "Serviço Teste", valor);

    private static OrdemServicoProduto CriarProduto(Guid idOs, decimal valorUnitario, decimal quantidade = 1m) =>
        new(idOs, Guid.NewGuid(), "Produto Teste", valorUnitario, quantidade);

    private static OrdemServico OSRecebida(decimal valorServico = 100m)
    {
        var os = new OrdemServico();
        os.Inserir(Guid.NewGuid(), Guid.NewGuid(), new List<OrdemServicoServico> { CriarServico(valorServico) });
        return os;
    }

    private static OrdemServico OSEmDiagnostico(decimal valorServico = 100m)
    {
        var os = OSRecebida(valorServico);
        os.IniciarDiagnostico();
        return os;
    }

    private static OrdemServico OSAguardandoAprovacao()
    {
        var os = OSEmDiagnostico();
        os.FinalizarDiagnostico();
        return os;
    }

    private static OrdemServico OSEmExecucao(params OrdemServicoServico[] servicosExtras)
    {
        var servicoBase = CriarServico(100m);
        var todos = new List<OrdemServicoServico> { servicoBase };
        todos.AddRange(servicosExtras);

        var os = new OrdemServico();
        os.Inserir(Guid.NewGuid(), Guid.NewGuid(), todos);
        os.IniciarDiagnostico();
        os.FinalizarDiagnostico();
        os.AprovarOrcamento();
        return os;
    }

    private static OrdemServico OSFinalizada()
    {
        var servico = CriarServico(100m);
        var os = new OrdemServico();
        os.Inserir(Guid.NewGuid(), Guid.NewGuid(), new List<OrdemServicoServico> { servico });
        os.IniciarDiagnostico();
        os.FinalizarDiagnostico();
        os.AprovarOrcamento();
        os.IniciarExecucaoServico(servico.Id);
        os.FinalizarExecucaoServico(servico.Id);
        return os;
    }

    /// <summary>
    /// Força um Status via reflexão para testar guards sem percorrer todo o fluxo.
    /// </summary>
    private static OrdemServico OSEmStatus(StatusOrdemServico status)
    {
        var os = OSRecebida();
        typeof(OrdemServico)
            .GetProperty(nameof(OrdemServico.Status))!
            .SetValue(os, status);
        return os;
    }
}