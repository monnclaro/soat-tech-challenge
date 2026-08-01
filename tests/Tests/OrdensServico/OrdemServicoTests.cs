using Domain.Common.Exceptions;
using Domain.OrdensServico;
using Domain.OrdensServico.Enums;
using Domain.OrdensServico.Events;
using Domain.OrdensServico.Produtos;
using Domain.OrdensServico.Servicos;
using Domain.OrdensServico.Servicos.Enums;

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

        var ordemServico = new OrdemServico();
        ordemServico.Inserir(idCliente, idVeiculo, servicos, new List<OrdemServicoProduto>());

        Assert.NotEqual(Guid.Empty, ordemServico.Id);
        Assert.Equal(idCliente, ordemServico.IdCliente);
        Assert.Equal(idVeiculo, ordemServico.IdVeiculo);
        Assert.Equal(StatusOrdemServico.Recebida, ordemServico.Status);
        Assert.True(ordemServico.DataCriacao >= antes);
        Assert.Null(ordemServico.DataInicioExecucao);
        Assert.Null(ordemServico.DataFinalizacao);
        Assert.Single(ordemServico.Servicos);
        Assert.Equal(100m, ordemServico.ValorTotal);
    }

    [Fact]
    public void Inserir_RaiseOrdemServicoStatusAlteradoDomainEvent_ComStatusRecebida()
    {
        var ordemServico = new OrdemServico();
        ordemServico.Inserir(Guid.NewGuid(), Guid.NewGuid(), new List<OrdemServicoServico> { CriarServico(100m) }, new List<OrdemServicoProduto>());

        var evento = Assert.Single(ordemServico.DomainEvents.OfType<OrdemServicoStatusAlteradoDomainEvent>());
        Assert.Equal(ordemServico.Id, evento.IdOrdemServico);
        Assert.Equal(StatusOrdemServico.Recebida, evento.Status);
    }

    [Fact]
    public void Inserir_CalculaValorTotalSomenteDosServicos()
    {
        var servicos = new List<OrdemServicoServico>
        {
            CriarServico(200m),
            CriarServico(300m),
        };

        var ordemServico = new OrdemServico();
        ordemServico.Inserir(Guid.NewGuid(), Guid.NewGuid(), servicos, new List<OrdemServicoProduto>());

        Assert.Equal(500m, ordemServico.ValorTotal);
        Assert.Empty(ordemServico.Produtos);
    }

    // ────────────────────────────────────────────────────────────
    // IniciarDiagnostico
    // ────────────────────────────────────────────────────────────

    [Fact]
    public void IniciarDiagnostico_QuandoStatusRecebida_MudaParaEmDiagnostico()
    {
        var ordemServico = OSRecebida();
        ordemServico.IniciarDiagnostico();

        Assert.Equal(StatusOrdemServico.EmDiagnostico, ordemServico.Status);
    }

    [Fact]
    public void IniciarDiagnostico_RaiseOrdemServicoStatusAlteradoDomainEvent_ComStatusEmDiagnostico()
    {
        var ordemServico = OSRecebida();

        ordemServico.IniciarDiagnostico();

        var evento = Assert.Single(ordemServico.DomainEvents
            .OfType<OrdemServicoStatusAlteradoDomainEvent>()
            .Where(e => e.Status == StatusOrdemServico.EmDiagnostico));
        Assert.Equal(ordemServico.Id, evento.IdOrdemServico);
    }

    [Theory]
    [InlineData(StatusOrdemServico.EmDiagnostico)]
    [InlineData(StatusOrdemServico.AguardandoAprovacao)]
    [InlineData(StatusOrdemServico.EmExecucao)]
    [InlineData(StatusOrdemServico.Finalizada)]
    [InlineData(StatusOrdemServico.Entregue)]
    public void IniciarDiagnostico_QuandoStatusDiferenteDeRecebida_LancaDomainException(StatusOrdemServico status)
    {
        var ordemServico = OSEmStatus(status);
        Assert.Throws<DomainException>(() => ordemServico.IniciarDiagnostico());
    }

    // ────────────────────────────────────────────────────────────
    // InserirProdutos
    // ────────────────────────────────────────────────────────────

    [Fact]
    public void InserirProdutos_QuandoEmDiagnostico_AdicionaProdutosERecalculaTotal()
    {
        var ordemServico = OSEmDiagnostico(valorServico: 100m);
        var produtos = new List<OrdemServicoProduto>
        {
            CriarProduto(ordemServico.Id, 50m),
            CriarProduto(ordemServico.Id, 30m),
        };

        ordemServico.InserirProdutos(produtos);

        Assert.Equal(2, ordemServico.Produtos.Count);
        Assert.Equal(180m, ordemServico.ValorTotal);
    }

    [Theory]
    [InlineData(StatusOrdemServico.Recebida)]
    [InlineData(StatusOrdemServico.AguardandoAprovacao)]
    [InlineData(StatusOrdemServico.EmExecucao)]
    [InlineData(StatusOrdemServico.Finalizada)]
    [InlineData(StatusOrdemServico.Entregue)]
    public void InserirProdutos_QuandoStatusDiferenteDeEmDiagnostico_LancaDomainException(StatusOrdemServico status)
    {
        var ordemServico = OSEmStatus(status);
        Assert.Throws<DomainException>(() =>
            ordemServico.InserirProdutos(new List<OrdemServicoProduto> { CriarProduto(ordemServico.Id, 10m) }));
    }

    // ────────────────────────────────────────────────────────────
    // InserirServicos
    // ────────────────────────────────────────────────────────────

    [Fact]
    public void InserirServicos_QuandoEmDiagnostico_AdicionaServicosERecalculaTotal()
    {
        var ordemServico = OSEmDiagnostico(valorServico: 100m);
        ordemServico.InserirServicos(new List<OrdemServicoServico> { CriarServico(ordemServico.Id, 200m) });

        Assert.Equal(2, ordemServico.Servicos.Count);
        Assert.Equal(300m, ordemServico.ValorTotal);
    }

    [Theory]
    [InlineData(StatusOrdemServico.Recebida)]
    [InlineData(StatusOrdemServico.AguardandoAprovacao)]
    [InlineData(StatusOrdemServico.EmExecucao)]
    [InlineData(StatusOrdemServico.Finalizada)]
    [InlineData(StatusOrdemServico.Entregue)]
    public void InserirServicos_QuandoStatusDiferenteDeEmDiagnostico_LancaDomainException(StatusOrdemServico status)
    {
        var ordemServico = OSEmStatus(status);
        Assert.Throws<DomainException>(() =>
            ordemServico.InserirServicos(new List<OrdemServicoServico> { CriarServico(ordemServico.Id, 10m) }));
    }

    // ────────────────────────────────────────────────────────────
    // RemoverProduto
    // ────────────────────────────────────────────────────────────

    [Fact]
    public void RemoverProduto_QuandoEmDiagnosticoEProdutoExiste_RemoveEAtualizaTotal()
    {
        var ordemServico = OSEmDiagnostico(valorServico: 100m);
        var produto = CriarProduto(ordemServico.Id, 50m);
        ordemServico.InserirProdutos(new List<OrdemServicoProduto> { produto });

        ordemServico.RemoverProduto(produto.Id);

        Assert.Empty(ordemServico.Produtos);
        Assert.Equal(100m, ordemServico.ValorTotal);
    }

    [Fact]
    public void RemoverProduto_QuandoProdutoNaoVinculado_LancaDomainException()
    {
        var ordemServico = OSEmDiagnostico();
        Assert.Throws<DomainException>(() => ordemServico.RemoverProduto(Guid.NewGuid()));
    }

    [Theory]
    [InlineData(StatusOrdemServico.Recebida)]
    [InlineData(StatusOrdemServico.AguardandoAprovacao)]
    [InlineData(StatusOrdemServico.EmExecucao)]
    [InlineData(StatusOrdemServico.Finalizada)]
    [InlineData(StatusOrdemServico.Entregue)]
    public void RemoverProduto_QuandoStatusDiferenteDeEmDiagnostico_LancaDomainException(StatusOrdemServico status)
    {
        var ordemServico = OSEmStatus(status);
        Assert.Throws<DomainException>(() => ordemServico.RemoverProduto(Guid.NewGuid()));
    }

    // ────────────────────────────────────────────────────────────
    // RemoverServico
    // ────────────────────────────────────────────────────────────

    [Fact]
    public void RemoverServico_QuandoEmDiagnosticoEServicoExiste_RemoveEAtualizaTotal()
    {
        var ordemServico = OSEmDiagnostico(valorServico: 100m);
        var novoServico = CriarServico(ordemServico.Id, 200m);
        ordemServico.InserirServicos(new List<OrdemServicoServico> { novoServico });

        ordemServico.RemoverServico(novoServico.Id);

        Assert.Single(ordemServico.Servicos);
        Assert.Equal(100m, ordemServico.ValorTotal);
    }

    [Fact]
    public void RemoverServico_QuandoServicoNaoVinculado_LancaDomainException()
    {
        var ordemServico = OSEmDiagnostico();
        Assert.Throws<DomainException>(() => ordemServico.RemoverServico(Guid.NewGuid()));
    }

    [Theory]
    [InlineData(StatusOrdemServico.Recebida)]
    [InlineData(StatusOrdemServico.AguardandoAprovacao)]
    [InlineData(StatusOrdemServico.EmExecucao)]
    [InlineData(StatusOrdemServico.Finalizada)]
    [InlineData(StatusOrdemServico.Entregue)]
    public void RemoverServico_QuandoStatusDiferenteDeEmDiagnostico_LancaDomainException(StatusOrdemServico status)
    {
        var ordemServico = OSEmStatus(status);
        Assert.Throws<DomainException>(() => ordemServico.RemoverServico(Guid.NewGuid()));
    }

    // ────────────────────────────────────────────────────────────
    // FinalizarDiagnostico / EnviarOrcamento
    // ────────────────────────────────────────────────────────────

    [Fact]
    public void FinalizarDiagnostico_QuandoEmDiagnosticoComServicos_MudaParaAguardandoAprovacao()
    {
        var ordemServico = OSEmDiagnostico();
        ordemServico.FinalizarDiagnostico();

        Assert.Equal(StatusOrdemServico.AguardandoAprovacao, ordemServico.Status);
    }

    [Fact]
    public void FinalizarDiagnostico_RaiseOrdemServicoStatusAlteradoDomainEvent_ComStatusAguardandoAprovacao()
    {
        var ordemServico = OSEmDiagnostico();

        ordemServico.FinalizarDiagnostico();

        var evento = Assert.Single(ordemServico.DomainEvents
            .OfType<OrdemServicoStatusAlteradoDomainEvent>()
            .Where(e => e.Status == StatusOrdemServico.AguardandoAprovacao));
        Assert.Equal(ordemServico.Id, evento.IdOrdemServico);
    }

    [Fact]
    public void FinalizarDiagnostico_QuandoSemServicos_LancaDomainException()
    {
        var ordemServico = new OrdemServico();
        ordemServico.Inserir(Guid.NewGuid(), Guid.NewGuid(), new List<OrdemServicoServico>(), new List<OrdemServicoProduto>());
        ordemServico.IniciarDiagnostico();

        Assert.Throws<DomainException>(() => ordemServico.FinalizarDiagnostico());
    }

    [Fact]
    public void FinalizarDiagnostico_QuandoStatusRecebida_LancaDomainException()
    {
        var ordemServico = OSRecebida();
        Assert.Throws<DomainException>(() => ordemServico.FinalizarDiagnostico());
    }

    // ────────────────────────────────────────────────────────────
    // AprovarOrcamento
    // ────────────────────────────────────────────────────────────

    [Fact]
    public void AprovarOrcamento_QuandoAguardandoAprovacao_MudaParaEmExecucaoESetaDataInicio()
    {
        var ordemServico = OSAguardandoAprovacao();
        var antes = DateTime.UtcNow;

        ordemServico.AprovarOrcamento();

        Assert.Equal(StatusOrdemServico.EmExecucao, ordemServico.Status);
        Assert.NotNull(ordemServico.DataInicioExecucao);
        Assert.True(ordemServico.DataInicioExecucao >= antes);
    }

    [Fact]
    public void AprovarOrcamento_RaiseOrdemServicoStatusAlteradoDomainEvent_ComStatusEmExecucao()
    {
        var ordemServico = OSAguardandoAprovacao();

        ordemServico.AprovarOrcamento();

        var evento = Assert.Single(ordemServico.DomainEvents
            .OfType<OrdemServicoStatusAlteradoDomainEvent>()
            .Where(e => e.Status == StatusOrdemServico.EmExecucao));
        Assert.Equal(ordemServico.Id, evento.IdOrdemServico);
    }

    [Theory]
    [InlineData(StatusOrdemServico.Recebida)]
    [InlineData(StatusOrdemServico.EmDiagnostico)]
    [InlineData(StatusOrdemServico.EmExecucao)]
    [InlineData(StatusOrdemServico.Finalizada)]
    [InlineData(StatusOrdemServico.Entregue)]
    public void AprovarOrcamento_QuandoStatusDiferenteDeAguardandoAprovacao_LancaDomainException(StatusOrdemServico status)
    {
        var ordemServico = OSEmStatus(status);
        Assert.Throws<DomainException>(() => ordemServico.AprovarOrcamento());
    }

    // ────────────────────────────────────────────────────────────
    // IniciarExecucaoServico
    // ────────────────────────────────────────────────────────────

    [Fact]
    public void IniciarExecucaoServico_QuandoEmExecucaoEServicoExiste_IniciaExecucaoDoServico()
    {
        var servico = CriarServico(100m);
        var ordemServico = OSEmExecucao(servico);

        ordemServico.IniciarExecucaoServico(servico.Id);

        Assert.Equal(StatusOrdemServicoServico.EmExecucao, servico.Status);
        Assert.NotNull(servico.DataInicioExecucao);
    }

    [Fact]
    public void IniciarExecucaoServico_QuandoServicoNaoVinculado_LancaDomainException()
    {
        var ordemServico = OSEmExecucao();
        Assert.Throws<DomainException>(() => ordemServico.IniciarExecucaoServico(Guid.NewGuid()));
    }

    [Theory]
    [InlineData(StatusOrdemServico.Recebida)]
    [InlineData(StatusOrdemServico.EmDiagnostico)]
    [InlineData(StatusOrdemServico.AguardandoAprovacao)]
    [InlineData(StatusOrdemServico.Finalizada)]
    [InlineData(StatusOrdemServico.Entregue)]
    public void IniciarExecucaoServico_QuandoStatusDiferenteDeEmExecucao_LancaDomainException(StatusOrdemServico status)
    {
        var ordemServico = OSEmStatus(status);
        Assert.Throws<DomainException>(() => ordemServico.IniciarExecucaoServico(Guid.NewGuid()));
    }

    // ────────────────────────────────────────────────────────────
    // FinalizarExecucaoServico
    // ────────────────────────────────────────────────────────────

    [Fact]
    public void FinalizarExecucaoServico_QuandoUltimoServico_FinalizaOSESetaDataFinalizacao()
    {
        var servico = CriarServico(100m);
        
        var ordemServico = OSEmExecucao(servico);
        var antes = DateTime.UtcNow;
        
        ordemServico.IniciarExecucaoServico(ordemServico.Servicos[0].Id);
        ordemServico.FinalizarExecucaoServico(ordemServico.Servicos[0].Id);
        ordemServico.IniciarExecucaoServico(ordemServico.Servicos[1].Id);
        ordemServico.FinalizarExecucaoServico(ordemServico.Servicos[1].Id);
        
        Assert.Equal(StatusOrdemServico.Finalizada, ordemServico.Status);
        Assert.NotNull(ordemServico.DataFinalizacao);
        Assert.True(ordemServico.DataFinalizacao >= antes);
    }

    [Fact]
    public void FinalizarExecucaoServico_QuandoUltimoServico_RaiseOrdemServicoStatusAlteradoDomainEvent_ComStatusFinalizada()
    {
        var servico = CriarServico(100m);
        var ordemServico = OSEmExecucao(servico);

        ordemServico.IniciarExecucaoServico(ordemServico.Servicos[0].Id);
        ordemServico.FinalizarExecucaoServico(ordemServico.Servicos[0].Id);
        ordemServico.IniciarExecucaoServico(ordemServico.Servicos[1].Id);
        ordemServico.FinalizarExecucaoServico(ordemServico.Servicos[1].Id);

        var evento = Assert.Single(ordemServico.DomainEvents
            .OfType<OrdemServicoStatusAlteradoDomainEvent>()
            .Where(e => e.Status == StatusOrdemServico.Finalizada));
        Assert.Equal(ordemServico.Id, evento.IdOrdemServico);
    }

    [Fact]
    public void FinalizarExecucaoServico_QuandoAindaHaServicoPendente_NaoFinalizaOS()
    {
        var s1 = CriarServico(100m);
        var s2 = CriarServico(200m);
        var ordemServico = OSEmExecucao(s1, s2);

        ordemServico.IniciarExecucaoServico(s1.Id);
        ordemServico.FinalizarExecucaoServico(s1.Id);

        Assert.Equal(StatusOrdemServico.EmExecucao, ordemServico.Status);
        Assert.Null(ordemServico.DataFinalizacao);
    }

    [Fact]
    public void FinalizarExecucaoServico_QuandoServicoNaoVinculado_LancaDomainException()
    {
        var ordemServico = OSEmExecucao();
        Assert.Throws<DomainException>(() => ordemServico.FinalizarExecucaoServico(Guid.NewGuid()));
    }

    [Theory]
    [InlineData(StatusOrdemServico.Recebida)]
    [InlineData(StatusOrdemServico.EmDiagnostico)]
    [InlineData(StatusOrdemServico.AguardandoAprovacao)]
    [InlineData(StatusOrdemServico.Finalizada)]
    [InlineData(StatusOrdemServico.Entregue)]
    public void FinalizarExecucaoServico_QuandoStatusDiferenteDeEmExecucao_LancaDomainException(StatusOrdemServico status)
    {
        var ordemServico = OSEmStatus(status);
        Assert.Throws<DomainException>(() => ordemServico.FinalizarExecucaoServico(Guid.NewGuid()));
    }

    // ────────────────────────────────────────────────────────────
    // Entregar
    // ────────────────────────────────────────────────────────────

    [Fact]
    public void Entregar_QuandoFinalizada_MudaParaEntregue()
    {
        var ordemServico = OSFinalizada();
        ordemServico.Entregar();

        Assert.Equal(StatusOrdemServico.Entregue, ordemServico.Status);
    }

    [Fact]
    public void Entregar_RaiseOrdemServicoStatusAlteradoDomainEvent_ComStatusEntregue()
    {
        var ordemServico = OSFinalizada();

        ordemServico.Entregar();

        var evento = Assert.Single(ordemServico.DomainEvents
            .OfType<OrdemServicoStatusAlteradoDomainEvent>()
            .Where(e => e.Status == StatusOrdemServico.Entregue));
        Assert.Equal(ordemServico.Id, evento.IdOrdemServico);
    }

    [Theory]
    [InlineData(StatusOrdemServico.Recebida)]
    [InlineData(StatusOrdemServico.EmDiagnostico)]
    [InlineData(StatusOrdemServico.AguardandoAprovacao)]
    [InlineData(StatusOrdemServico.EmExecucao)]
    [InlineData(StatusOrdemServico.Entregue)]
    public void Entregar_QuandoStatusDiferenteDeFinalizada_LancaDomainException(StatusOrdemServico status)
    {
        var ordemServico = OSEmStatus(status);
        Assert.Throws<DomainException>(() => ordemServico.Entregar());
    }

    // ────────────────────────────────────────────────────────────
    // IdsProdutos / IdsServicos (computed)
    // ────────────────────────────────────────────────────────────

    [Fact]
    public void IdsProdutos_RetornaIdsProdutoDosProdutosVinculados()
    {
        var ordemServico = OSEmDiagnostico();
        var p1 = CriarProduto(ordemServico.Id, 10m);
        var p2 = CriarProduto(ordemServico.Id, 20m);
        ordemServico.InserirProdutos(new List<OrdemServicoProduto> { p1, p2 });

        Assert.Contains(p1.IdProduto, ordemServico.IdsProdutos);
        Assert.Contains(p2.IdProduto, ordemServico.IdsProdutos);
        Assert.Equal(2, ordemServico.IdsProdutos.Count);
    }

    [Fact]
    public void IdsServicos_RetornaIdServicoDosSericosVinculados()
    {
        var s1 = CriarServico(100m);
        var s2 = CriarServico(200m);
        var ordemServico = new OrdemServico();
        ordemServico.Inserir(Guid.NewGuid(), Guid.NewGuid(), new List<OrdemServicoServico> { s1, s2 }, new List<OrdemServicoProduto>());

        Assert.Contains(s1.IdServico, ordemServico.IdsServicos);
        Assert.Contains(s2.IdServico, ordemServico.IdsServicos);
        Assert.Equal(2, ordemServico.IdsServicos.Count);
    }

    // ────────────────────────────────────────────────────────────
    // Fluxo completo (smoke test)
    // ────────────────────────────────────────────────────────────

    [Fact]
    public void FluxoCompleto_DeRecebidaAteEntregue_SemExcecoes()
    {
        var servico = CriarServico(500m);
        var ordemServico = new OrdemServico();
        ordemServico.Inserir(Guid.NewGuid(), Guid.NewGuid(), new List<OrdemServicoServico> { servico }, new List<OrdemServicoProduto>());

        ordemServico.IniciarDiagnostico();
        ordemServico.InserirProdutos(new List<OrdemServicoProduto> { CriarProduto(ordemServico.Id, 100m) });
        ordemServico.FinalizarDiagnostico();
        ordemServico.AprovarOrcamento();
        ordemServico.IniciarExecucaoServico(servico.Id);
        ordemServico.FinalizarExecucaoServico(servico.Id);
        ordemServico.Entregar();

        Assert.Equal(StatusOrdemServico.Entregue, ordemServico.Status);
        Assert.Equal(600m, ordemServico.ValorTotal);
        Assert.NotNull(ordemServico.DataInicioExecucao);
        Assert.NotNull(ordemServico.DataFinalizacao);
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
        var ordemServico = new OrdemServico();
        ordemServico.Inserir(Guid.NewGuid(), Guid.NewGuid(), new List<OrdemServicoServico> { CriarServico(valorServico) }, new List<OrdemServicoProduto>());
        return ordemServico;
    }

    private static OrdemServico OSEmDiagnostico(decimal valorServico = 100m)
    {
        var ordemServico = OSRecebida(valorServico);
        ordemServico.IniciarDiagnostico();
        return ordemServico;
    }

    private static OrdemServico OSAguardandoAprovacao()
    {
        var ordemServico = OSEmDiagnostico();
        ordemServico.FinalizarDiagnostico();
        return ordemServico;
    }

    private static OrdemServico OSEmExecucao(params OrdemServicoServico[] servicosExtras)
    {
        var servicoBase = CriarServico(100m);
        var todos = new List<OrdemServicoServico> { servicoBase };
        todos.AddRange(servicosExtras);

        var ordemServico = new OrdemServico();
        ordemServico.Inserir(Guid.NewGuid(), Guid.NewGuid(), todos, new List<OrdemServicoProduto>());
        ordemServico.IniciarDiagnostico();
        ordemServico.FinalizarDiagnostico();
        ordemServico.AprovarOrcamento();
        return ordemServico;
    }

    private static OrdemServico OSFinalizada()
    {
        var servico = CriarServico(100m);
        var ordemServico = new OrdemServico();
        ordemServico.Inserir(Guid.NewGuid(), Guid.NewGuid(), new List<OrdemServicoServico> { servico }, new List<OrdemServicoProduto>());
        ordemServico.IniciarDiagnostico();
        ordemServico.FinalizarDiagnostico();
        ordemServico.AprovarOrcamento();
        ordemServico.IniciarExecucaoServico(servico.Id);
        ordemServico.FinalizarExecucaoServico(servico.Id);
        return ordemServico;
    }

    /// <summary>
    /// Força um Status via reflexão para testar guards sem percorrer todo o fluxo.
    /// </summary>
    private static OrdemServico OSEmStatus(StatusOrdemServico status)
    {
        var ordemServico = OSRecebida();
        typeof(OrdemServico)
            .GetProperty(nameof(OrdemServico.Status))!
            .SetValue(ordemServico, status);
        
        return ordemServico;
    }
}