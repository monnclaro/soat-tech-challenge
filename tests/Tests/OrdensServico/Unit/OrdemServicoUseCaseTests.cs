using Application.OrdensServico.Queries;
using Application.OrdensServico.UseCases;
using Application.OrdensServico.UseCases.AprovarOrcamento;
using Application.OrdensServico.UseCases.BuscarOrdemServico;
using Application.OrdensServico.UseCases.BuscarStatus;
using Application.OrdensServico.UseCases.Entregar;
using Application.OrdensServico.UseCases.FinalizarDiagnostico;
using Application.OrdensServico.UseCases.FinalizarExecucaoServico;
using Application.OrdensServico.UseCases.IniciarDiagnostico;
using Application.OrdensServico.UseCases.IniciarExecucaoServico;
using Application.OrdensServico.UseCases.InserirOrdemServico;
using Application.OrdensServico.UseCases.InserirProdutos;
using Application.OrdensServico.UseCases.InserirServicos;
using Application.OrdensServico.UseCases.Remover;
using Application.OrdensServico.UseCases.RemoverProduto;
using Application.OrdensServico.UseCases.RemoverServico;
using Application.OrdensServico.UseCases.ReprovarOrcamento;
using Domain.Clientes;
using Domain.Clientes.Gateways;
using Domain.Clientes.ValueObjects;
using Domain.Clientes.Veiculos;
using Domain.Clientes.Veiculos.ValueObjects;
using Domain.Common.Exceptions;
using Domain.OrdensServico;
using Domain.OrdensServico.Enums;
using Domain.OrdensServico.Gateways;
using Domain.OrdensServico.Produtos;
using Domain.OrdensServico.Servicos;
using Domain.OrdensServico.Servicos.Enums;
using Domain.Produtos;
using Domain.Servicos;
using SharedKernel.DTOs;

using Tests.Produtos.Unit;
using Tests.Servicos.Unit;

namespace Tests.OrdensServico.Unit;

public class OrdemServicoUseCaseTests
{
    private static readonly int AnoAtual = DateTime.Now.Year;

    // ── BuscarOrdemServico ───────────────────────────────────────

    [Fact]
    public async Task Buscar_QuandoNaoExiste_ChamaNaoEncontrado()
    {
        var presenter = new FakeBuscarOrdemServicoPresenter();
        var useCase   = new BuscarOrdemServicoUseCase(new FakeOrdemServicoQueryGateway(), presenter);

        await useCase.Execute(new BuscarOrdemServicoInput(Guid.NewGuid()), CancellationToken.None);

        Assert.True(presenter.NaoEncontradoChamado);
    }

    [Fact]
    public async Task Buscar_QuandoExiste_ChamaOk()
    {
        var output    = CriarOrdemServicoOutput();
        var presenter = new FakeBuscarOrdemServicoPresenter();
        var useCase   = new BuscarOrdemServicoUseCase(new FakeOrdemServicoQueryGateway(output), presenter);

        await useCase.Execute(new BuscarOrdemServicoInput(output.Id), CancellationToken.None);

        Assert.False(presenter.NaoEncontradoChamado);
        Assert.Equal(output.Id, presenter.Output?.Id);
    }

    // ── BuscarStatus ─────────────────────────────────────────────

    [Fact]
    public async Task BuscarStatus_QuandoNaoExiste_ChamaNaoEncontrado()
    {
        var presenter = new FakeBuscarStatusPresenter();
        var useCase   = new BuscarStatusUseCase(new FakeOrdemServicoQueryGateway(), presenter);

        await useCase.Execute(new BuscarStatusInput(Guid.NewGuid()), CancellationToken.None);

        Assert.True(presenter.NaoEncontradoChamado);
    }

    // ── Inserir ──────────────────────────────────────────────────

    [Fact]
    public async Task Inserir_QuandoClienteNaoExiste_ChamaClienteNaoEncontrado()
    {
        var (useCase, os, presenter) = new InserirOrdemServicoBuilder().Build();

        await useCase.Execute(new InserirOrdemServicoInput(Guid.NewGuid(), Guid.NewGuid(), [], []), CancellationToken.None);

        Assert.True(presenter.ClienteNaoEncontradoChamado);
        Assert.False(os.SalvarFoiChamado);
    }

    [Fact]
    public async Task Inserir_QuandoVeiculoNaoPertenceAoCliente_ChamaVeiculoNaoPertence()
    {
        var cliente      = CriarCliente();
        var (useCase, os, presenter) = new InserirOrdemServicoBuilder()
            .ComCliente(cliente)
            .Build();

        await useCase.Execute(new InserirOrdemServicoInput(cliente.Id, Guid.NewGuid(), [], []), CancellationToken.None);

        Assert.True(presenter.VeiculoNaoPertenceChamado);
        Assert.False(os.SalvarFoiChamado);
    }

    [Fact]
    public async Task Inserir_QuandoDadosValidos_ChamaOkEPersiste()
    {
        var cliente      = CriarClienteComVeiculo();
        var (useCase, os, presenter) = new InserirOrdemServicoBuilder()
            .ComCliente(cliente)
            .Build();

        await useCase.Execute(new InserirOrdemServicoInput(cliente.Id, cliente.Veiculos[0].Id, [], []), CancellationToken.None);

        Assert.True(presenter.OkChamado);
        Assert.True(os.SalvarFoiChamado);
        Assert.NotEqual(Guid.Empty, presenter.IdCriado);
    }

    [Fact]
    public async Task Inserir_ComServicosValidos_AdicionaServicosNaOS()
    {
        var cliente  = CriarClienteComVeiculo();
        var s1       = CriarServico("Alinhamento", 200m);
        var s2       = CriarServico("Balanceamento", 150m);
        var (useCase, os, presenter) = new InserirOrdemServicoBuilder()
            .ComCliente(cliente)
            .ComServicos(s1, s2)
            .Build();

        await useCase.Execute(
            new InserirOrdemServicoInput(cliente.Id, cliente.Veiculos[0].Id, [s1.Id, s2.Id], []),
            CancellationToken.None);

        Assert.True(presenter.OkChamado);
        Assert.Equal(2, os.OsSalva!.Servicos.Count);
        Assert.Contains(os.OsSalva.Servicos, s => s.IdServico == s1.Id && s.NomeServico == "Alinhamento" && s.Valor == 200m);
        Assert.Contains(os.OsSalva.Servicos, s => s.IdServico == s2.Id && s.NomeServico == "Balanceamento" && s.Valor == 150m);
    }

    [Fact]
    public async Task Inserir_ComProdutosValidos_AdicionaProdutosNaOS()
    {
        var cliente  = CriarClienteComVeiculo();
        var produto  = CriarProduto(estoque: 10);
        var (useCase, os, presenter) = new InserirOrdemServicoBuilder()
            .ComCliente(cliente)
            .ComProdutos(produto)
            .Build();

        await useCase.Execute(
            new InserirOrdemServicoInput(cliente.Id, cliente.Veiculos[0].Id, [],
                [new InserirOrdemServicoProdutoInput(produto.Id, 3)]),
            CancellationToken.None);

        Assert.True(presenter.OkChamado);
        Assert.Single(os.OsSalva!.Produtos);
        Assert.Equal(produto.Id, os.OsSalva.Produtos[0].IdProduto);
        Assert.Equal(3, os.OsSalva.Produtos[0].Quantidade);
    }

    [Fact]
    public async Task Inserir_ComServicosEProdutos_AdicionaAmbosNaOS()
    {
        var cliente  = CriarClienteComVeiculo();
        var servico  = CriarServico();
        var produto  = CriarProduto(estoque: 10);
        var (useCase, os, _) = new InserirOrdemServicoBuilder()
            .ComCliente(cliente)
            .ComServicos(servico)
            .ComProdutos(produto)
            .Build();

        await useCase.Execute(
            new InserirOrdemServicoInput(cliente.Id, cliente.Veiculos[0].Id,
                [servico.Id],
                [new InserirOrdemServicoProdutoInput(produto.Id, 2)]),
            CancellationToken.None);

        Assert.Single(os.OsSalva!.Servicos);
        Assert.Single(os.OsSalva.Produtos);
    }

    [Fact]
    public async Task Inserir_QuandoAlgunsIdsServicoNaoExistem_AdicionaSomenteOsEncontrados()
    {
        var cliente  = CriarClienteComVeiculo();
        var servico  = CriarServico();
        var (useCase, os, presenter) = new InserirOrdemServicoBuilder()
            .ComCliente(cliente)
            .ComServicos(servico)
            .Build();

        await useCase.Execute(
            new InserirOrdemServicoInput(cliente.Id, cliente.Veiculos[0].Id, [servico.Id, Guid.NewGuid()], []),
            CancellationToken.None);

        Assert.True(presenter.OkChamado);
        Assert.Single(os.OsSalva!.Servicos);
    }

    [Fact]
    public async Task Inserir_QuandoAlgunsIdsProdutoNaoExistem_AdicionaSomenteOsEncontrados()
    {
        var cliente  = CriarClienteComVeiculo();
        var produto  = CriarProduto(estoque: 10);
        var (useCase, os, presenter) = new InserirOrdemServicoBuilder()
            .ComCliente(cliente)
            .ComProdutos(produto)
            .Build();

        await useCase.Execute(
            new InserirOrdemServicoInput(cliente.Id, cliente.Veiculos[0].Id, [],
                [new InserirOrdemServicoProdutoInput(produto.Id, 1),
                 new InserirOrdemServicoProdutoInput(Guid.NewGuid(), 5)]),
            CancellationToken.None);

        Assert.True(presenter.OkChamado);
        Assert.Single(os.OsSalva!.Produtos);
    }

    [Fact]
    public async Task Inserir_OSVinculaIdClienteEIdVeiculoCorretamente()
    {
        var cliente  = CriarClienteComVeiculo();
        var (useCase, os, _) = new InserirOrdemServicoBuilder()
            .ComCliente(cliente)
            .Build();

        await useCase.Execute(
            new InserirOrdemServicoInput(cliente.Id, cliente.Veiculos[0].Id, [], []),
            CancellationToken.None);

        Assert.Equal(cliente.Id, os.OsSalva!.IdCliente);
        Assert.Equal(cliente.Veiculos[0].Id, os.OsSalva.IdVeiculo);
    }

    // ── IniciarDiagnostico ───────────────────────────────────────

    [Fact]
    public async Task IniciarDiagnostico_QuandoNaoExiste_ChamaNaoEncontrado()
    {
        var presenter = new FakeEstadoPresenter();
        var gateway   = new FakeOrdemServicoGateway();
        var useCase   = new IniciarDiagnosticoUseCase(gateway, presenter);

        await useCase.Execute(new IniciarDiagnosticoInput(Guid.NewGuid()), CancellationToken.None);

        Assert.True(presenter.NaoEncontradoChamado);
        Assert.False(gateway.AtualizarFoiChamado);
    }

    [Fact]
    public async Task IniciarDiagnostico_QuandoRecebida_MudaStatusEChamaOk()
    {
        var os        = CriarOrdemServicoRecebida();
        var presenter = new FakeEstadoPresenter();
        var gateway   = new FakeOrdemServicoGateway(os);
        var useCase   = new IniciarDiagnosticoUseCase(gateway, presenter);

        await useCase.Execute(new IniciarDiagnosticoInput(os.Id), CancellationToken.None);

        Assert.Equal(StatusOrdemServico.EmDiagnostico, os.Status);
        Assert.True(presenter.OkChamado);
        Assert.True(gateway.AtualizarFoiChamado);
    }

    // ── FinalizarDiagnostico ─────────────────────────────────────

    [Fact]
    public async Task FinalizarDiagnostico_QuandoNaoExiste_ChamaNaoEncontrado()
    {
        var (useCase, presenter) = new EstadoUseCaseBuilder<FinalizarDiagnosticoUseCase, FinalizarDiagnosticoInput>()
            .Build((gw, p) => new FinalizarDiagnosticoUseCase(gw, p));

        await useCase.Execute(new FinalizarDiagnosticoInput(Guid.NewGuid()), CancellationToken.None);

        Assert.True(presenter.NaoEncontradoChamado);
    }

    [Fact]
    public async Task FinalizarDiagnostico_QuandoEmDiagnostico_MudaParaAguardandoAprovacao()
    {
        var os = CriarOrdemServicoEmDiagnostico();
        var (useCase, presenter) = new EstadoUseCaseBuilder<FinalizarDiagnosticoUseCase, FinalizarDiagnosticoInput>()
            .ComOS(os)
            .Build((gw, p) => new FinalizarDiagnosticoUseCase(gw, p));

        await useCase.Execute(new FinalizarDiagnosticoInput(os.Id), CancellationToken.None);

        Assert.Equal(StatusOrdemServico.AguardandoAprovacao, os.Status);
        Assert.True(presenter.OkChamado);
    }

    // ── AprovarOrcamento ─────────────────────────────────────────

    [Fact]
    public async Task AprovarOrcamento_QuandoNaoExiste_ChamaNaoEncontrado()
    {
        var (useCase, presenter) = new EstadoUseCaseBuilder<AprovarOrcamentoUseCase, AprovarOrcamentoInput>()
            .Build((gw, p) => new AprovarOrcamentoUseCase(gw, p));

        await useCase.Execute(new AprovarOrcamentoInput(Guid.NewGuid()), CancellationToken.None);

        Assert.True(presenter.NaoEncontradoChamado);
    }

    [Fact]
    public async Task AprovarOrcamento_QuandoAguardandoAprovacao_MudaParaEmExecucao()
    {
        var os = CriarOrdemServicoAguardandoAprovacao();
        var (useCase, presenter) = new EstadoUseCaseBuilder<AprovarOrcamentoUseCase, AprovarOrcamentoInput>()
            .ComOS(os)
            .Build((gw, p) => new AprovarOrcamentoUseCase(gw, p));

        await useCase.Execute(new AprovarOrcamentoInput(os.Id), CancellationToken.None);

        Assert.Equal(StatusOrdemServico.EmExecucao, os.Status);
        Assert.True(presenter.OkChamado);
    }

    // ── ReprovarOrcamento ────────────────────────────────────────

    [Fact]
    public async Task ReprovarOrcamento_QuandoNaoExiste_ChamaNaoEncontrado()
    {
        var (useCase, presenter) = new EstadoUseCaseBuilder<ReprovarOrcamentoUseCase, ReprovarOrcamentoInput>()
            .Build((gw, p) => new ReprovarOrcamentoUseCase(gw, p));

        await useCase.Execute(new ReprovarOrcamentoInput(Guid.NewGuid()), CancellationToken.None);

        Assert.True(presenter.NaoEncontradoChamado);
    }

    [Fact]
    public async Task ReprovarOrcamento_QuandoAguardandoAprovacao_MudaParaReprovado()
    {
        var os = CriarOrdemServicoAguardandoAprovacao();
        var (useCase, presenter) = new EstadoUseCaseBuilder<ReprovarOrcamentoUseCase, ReprovarOrcamentoInput>()
            .ComOS(os)
            .Build((gw, p) => new ReprovarOrcamentoUseCase(gw, p));

        await useCase.Execute(new ReprovarOrcamentoInput(os.Id), CancellationToken.None);

        Assert.Equal(StatusOrdemServico.Finalizada, os.Status);
        Assert.True(presenter.OkChamado);
    }

    // ── IniciarExecucaoServico ───────────────────────────────────

    [Fact]
    public async Task IniciarExecucaoServico_QuandoNaoExiste_ChamaNaoEncontrado()
    {
        var presenter = new FakeEstadoPresenter();
        var useCase   = new IniciarExecucaoServicoUseCase(new FakeOrdemServicoGateway(), presenter);

        await useCase.Execute(new IniciarExecucaoServicoInput(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.True(presenter.NaoEncontradoChamado);
    }

    [Fact]
    public async Task IniciarExecucaoServico_QuandoEmExecucao_IniciaServico()
    {
        var servico   = new OrdemServicoServico(Guid.NewGuid(), Guid.NewGuid(), "Serviço", 100m);
        var os        = CriarOrdemServicoEmExecucao(servico);
        var presenter = new FakeEstadoPresenter();
        var useCase   = new IniciarExecucaoServicoUseCase(new FakeOrdemServicoGateway(os), presenter);

        await useCase.Execute(new IniciarExecucaoServicoInput(os.Id, servico.Id), CancellationToken.None);

        Assert.Equal(StatusOrdemServicoServico.EmExecucao, servico.Status);
        Assert.True(presenter.OkChamado);
    }

    // ── FinalizarExecucaoServico ─────────────────────────────────

    [Fact]
    public async Task FinalizarExecucaoServico_QuandoUltimoServico_FinalizaOS()
    {
        var servico   = new OrdemServicoServico(Guid.NewGuid(), Guid.NewGuid(), "Serviço", 100m);
        var os        = CriarOrdemServicoEmExecucaoUnico(servico);
        servico.IniciarExecucao();
        var presenter = new FakeEstadoPresenter();
        var useCase   = new FinalizarExecucaoServicoUseCase(new FakeOrdemServicoGateway(os), presenter);

        await useCase.Execute(new FinalizarExecucaoServicoInput(os.Id, servico.Id), CancellationToken.None);

        Assert.Equal(StatusOrdemServico.Finalizada, os.Status);
        Assert.True(presenter.OkChamado);
    }

    [Fact]
    public async Task FinalizarExecucaoServico_QuandoAindaHaServicoPendente_NaoFinaliza()
    {
        var s1 = new OrdemServicoServico(Guid.NewGuid(), Guid.NewGuid(), "S1", 100m);
        var s2 = new OrdemServicoServico(Guid.NewGuid(), Guid.NewGuid(), "S2", 100m);
        var os = CriarOrdemServicoEmExecucao(s1, s2);
        s1.IniciarExecucao();
        var useCase = new FinalizarExecucaoServicoUseCase(new FakeOrdemServicoGateway(os), new FakeEstadoPresenter());

        await useCase.Execute(new FinalizarExecucaoServicoInput(os.Id, s1.Id), CancellationToken.None);

        Assert.Equal(StatusOrdemServico.EmExecucao, os.Status);
    }

    // ── Entregar ─────────────────────────────────────────────────

    [Fact]
    public async Task Entregar_QuandoFinalizada_MudaParaEntregue()
    {
        var os        = CriarOrdemServicoFinalizada();
        var presenter = new FakeEstadoPresenter();
        var useCase   = new EntregarUseCase(new FakeOrdemServicoGateway(os), presenter);

        await useCase.Execute(new EntregarInput(os.Id), CancellationToken.None);

        Assert.Equal(StatusOrdemServico.Entregue, os.Status);
        Assert.True(presenter.OkChamado);
    }

    // ── InserirProdutos ──────────────────────────────────────────

    [Fact]
    public async Task InserirProdutos_QuandoNaoExiste_ChamaNaoEncontrado()
    {
        var (useCase, _, presenter) = new InserirProdutosBuilder().Build();

        await useCase.Execute(new InserirProdutosInput(Guid.NewGuid(), []), CancellationToken.None);

        Assert.True(presenter.NaoEncontradoChamado);
    }

    [Fact]
    public async Task InserirProdutos_QuandoEstoqueInsuficiente_ChamaEstoqueInsuficiente()
    {
        var os      = CriarOrdemServicoEmDiagnostico();
        var produto = CriarProduto(estoque: 2);
        var (useCase, _, presenter) = new InserirProdutosBuilder()
            .ComOS(os)
            .ComProdutos(produto)
            .Build();

        await useCase.Execute(new InserirProdutosInput(os.Id,
            [new InserirProdutosItemInput(produto.Id, 10)]), CancellationToken.None);

        Assert.True(presenter.EstoqueInsuficienteChamado);
    }

    [Fact]
    public async Task InserirProdutos_QuandoDadosValidos_ChamaOk()
    {
        var os      = CriarOrdemServicoEmDiagnostico();
        var produto = CriarProduto(estoque: 10);
        var (useCase, gateway, presenter) = new InserirProdutosBuilder()
            .ComOS(os)
            .ComProdutos(produto)
            .Build();

        await useCase.Execute(new InserirProdutosInput(os.Id,
            [new InserirProdutosItemInput(produto.Id, 3)]), CancellationToken.None);

        Assert.True(presenter.OkChamado);
        Assert.True(gateway.AtualizarFoiChamado);
    }

    // ── InserirServicos ──────────────────────────────────────────

    [Fact]
    public async Task InserirServicos_QuandoNaoExiste_ChamaNaoEncontrado()
    {
        var (useCase, gateway, presenter) = new InserirServicosBuilder().Build();

        await useCase.Execute(new InserirServicosOrdemServicoInput(Guid.NewGuid(), []), CancellationToken.None);

        Assert.True(presenter.NaoEncontradoChamado);
        Assert.False(gateway.AtualizarFoiChamado);
    }

    [Fact]
    public async Task InserirServicos_QuandoListaVazia_ChamaOkSemAdicionarServicos()
    {
        var os = CriarOrdemServicoEmDiagnostico();
        var (useCase, gateway, presenter) = new InserirServicosBuilder()
            .ComOS(os)
            .Build();
        var servicosAntes = os.Servicos.Count;

        await useCase.Execute(new InserirServicosOrdemServicoInput(os.Id, []), CancellationToken.None);

        Assert.True(presenter.OkChamado);
        Assert.Equal(servicosAntes, os.Servicos.Count);
        Assert.True(gateway.AtualizarFoiChamado);
    }

    [Fact]
    public async Task InserirServicos_QuandoServicosExistem_InsereDomainEChamaOk()
    {
        var os = CriarOrdemServicoEmDiagnostico();
        var s1 = CriarServico("Alinhamento", 200m);
        var s2 = CriarServico("Balanceamento", 150m);
        var (useCase, gateway, presenter) = new InserirServicosBuilder()
            .ComOS(os)
            .ComServicos(s1, s2)
            .Build();

        await useCase.Execute(new InserirServicosOrdemServicoInput(os.Id,
            [new InserirServicosOrdemServicoItemInput(s1.Id), new InserirServicosOrdemServicoItemInput(s2.Id)]),
            CancellationToken.None);

        Assert.True(presenter.OkChamado);
        Assert.False(presenter.NaoEncontradoChamado);
        Assert.Equal(3, os.Servicos.Count);
        Assert.True(gateway.AtualizarFoiChamado);
    }

    [Fact]
    public async Task InserirServicos_PreservaValorENomeDoServico()
    {
        var os      = CriarOrdemServicoEmDiagnostico();
        var servico = CriarServico("Troca de Óleo", 300m);
        var (useCase, _, _) = new InserirServicosBuilder()
            .ComOS(os)
            .ComServicos(servico)
            .Build();

        await useCase.Execute(new InserirServicosOrdemServicoInput(os.Id,
            [new InserirServicosOrdemServicoItemInput(servico.Id)]),
            CancellationToken.None);

        var inserido = os.Servicos.Last();
        Assert.Equal("Troca de Óleo", inserido.NomeServico);
        Assert.Equal(300m, inserido.Valor);
        Assert.Equal(os.Id, inserido.IdOrdemServico);
        Assert.Equal(servico.Id, inserido.IdServico);
    }

    [Fact]
    public async Task InserirServicos_QuandoAlgunsIdsNaoExistem_InsereSomenteOsEncontrados()
    {
        var os      = CriarOrdemServicoEmDiagnostico();
        var servico = CriarServico("Existente", 100m);
        var (useCase, _, presenter) = new InserirServicosBuilder()
            .ComOS(os)
            .ComServicos(servico)
            .Build();

        await useCase.Execute(new InserirServicosOrdemServicoInput(os.Id,
            [new InserirServicosOrdemServicoItemInput(servico.Id), new InserirServicosOrdemServicoItemInput(Guid.NewGuid())]),
            CancellationToken.None);

        Assert.True(presenter.OkChamado);
        Assert.Equal(2, os.Servicos.Count);
    }

    [Fact]
    public async Task InserirServicos_QuandoNenhumIdExiste_ChamaOkSemAdicionarServicos()
    {
        var os = CriarOrdemServicoEmDiagnostico();
        var (useCase, _, presenter) = new InserirServicosBuilder()
            .ComOS(os)
            .Build();
        var servicosAntes = os.Servicos.Count;

        await useCase.Execute(new InserirServicosOrdemServicoInput(os.Id,
            [new InserirServicosOrdemServicoItemInput(Guid.NewGuid()), new InserirServicosOrdemServicoItemInput(Guid.NewGuid())]),
            CancellationToken.None);

        Assert.True(presenter.OkChamado);
        Assert.Equal(servicosAntes, os.Servicos.Count);
    }

    [Fact]
    public async Task InserirServicos_QuandoOSNaoEmDiagnostico_PropagaDomainException()
    {
        var os      = CriarOrdemServicoRecebida();
        var servico = CriarServico();
        var (useCase, gateway, presenter) = new InserirServicosBuilder()
            .ComOS(os)
            .ComServicos(servico)
            .Build();

        await Assert.ThrowsAsync<DomainException>(() =>
            useCase.Execute(new InserirServicosOrdemServicoInput(os.Id,
                [new InserirServicosOrdemServicoItemInput(servico.Id)]),
                CancellationToken.None));

        Assert.False(presenter.OkChamado);
        Assert.False(gateway.AtualizarFoiChamado);
    }

    // ── Remover ──────────────────────────────────────────────────

    [Fact]
    public async Task Remover_QuandoNaoExiste_ChamaNaoEncontrado()
    {
        var presenter = new FakeEstadoPresenter();
        var useCase   = new RemoverOrdemServicoUseCase(new FakeOrdemServicoGateway(), presenter);

        await useCase.Execute(new RemoverOrdemServicoInput(Guid.NewGuid()), CancellationToken.None);

        Assert.True(presenter.NaoEncontradoChamado);
    }

    [Fact]
    public async Task Remover_QuandoExiste_RemoveEChamaOk()
    {
        var os        = CriarOrdemServicoRecebida();
        var gateway   = new FakeOrdemServicoGateway(os);
        var presenter = new FakeEstadoPresenter();
        var useCase   = new RemoverOrdemServicoUseCase(gateway, presenter);

        await useCase.Execute(new RemoverOrdemServicoInput(os.Id), CancellationToken.None);

        Assert.True(presenter.OkChamado);
        Assert.True(gateway.RemoverFoiChamado);
    }

    // ── RemoverProduto ───────────────────────────────────────────

    [Fact]
    public async Task RemoverProduto_QuandoNaoExiste_ChamaNaoEncontrado()
    {
        var presenter = new FakeEstadoPresenter();
        var useCase   = new RemoverProdutoUseCase(new FakeOrdemServicoGateway(), presenter);

        await useCase.Execute(new RemoverProdutoInput(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.True(presenter.NaoEncontradoChamado);
    }

    [Fact]
    public async Task RemoverProduto_QuandoExiste_RemoveEChamaOk()
    {
        var os        = CriarOrdemServicoEmDiagnostico();
        var produtoOs = new OrdemServicoProduto(os.Id, Guid.NewGuid(), "Produto", 50m, 1);
        os.Produtos.Add(produtoOs);
        var presenter = new FakeEstadoPresenter();
        var useCase   = new RemoverProdutoUseCase(new FakeOrdemServicoGateway(os), presenter);

        await useCase.Execute(new RemoverProdutoInput(os.Id, produtoOs.Id), CancellationToken.None);

        Assert.True(presenter.OkChamado);
        Assert.Empty(os.Produtos);
    }

    // ── RemoverServico ───────────────────────────────────────────

    [Fact]
    public async Task RemoverServico_QuandoNaoExiste_ChamaNaoEncontrado()
    {
        var presenter = new FakeEstadoPresenter();
        var useCase   = new RemoverServicoUseCase(new FakeOrdemServicoGateway(), presenter);

        await useCase.Execute(new RemoverServicoOrdemServicoInput(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.True(presenter.NaoEncontradoChamado);
    }

    [Fact]
    public async Task RemoverServico_QuandoExiste_RemoveEChamaOk()
    {
        var os        = CriarOrdemServicoEmDiagnostico();
        var servicoOs = new OrdemServicoServico(os.Id, Guid.NewGuid(), "Serviço Extra", 200m);
        os.Servicos.Add(servicoOs);
        var presenter = new FakeEstadoPresenter();
        var useCase   = new RemoverServicoUseCase(new FakeOrdemServicoGateway(os), presenter);

        await useCase.Execute(new RemoverServicoOrdemServicoInput(os.Id, servicoOs.Id), CancellationToken.None);

        Assert.True(presenter.OkChamado);
        Assert.DoesNotContain(os.Servicos, s => s.Id == servicoOs.Id);
    }

    // ── Helpers ──────────────────────────────────────────────────

    private static Cliente CriarCliente()
    {
        var c = new Cliente();
        c.Inserir("Cliente Teste", DocumentoCliente.Criar("52998224725"));
        return c;
    }

    private static Cliente CriarClienteComVeiculo()
    {
        var cliente = CriarCliente();
        var veiculo = new Veiculo();
        veiculo.Inserir(cliente.Id, Placa.Criar("ABC1234"), "Honda", "Civic", AnoAtual);
        cliente.Veiculos.Add(veiculo);
        return cliente;
    }

    private static Produto CriarProduto(int estoque = 10)
    {
        var p = new Produto();
        p.Inserir("Produto Teste", "Desc", 50m, estoque);
        return p;
    }

    private static Servico CriarServico(string nome = "Serviço Teste", decimal valor = 150m)
    {
        var s = new Servico();
        s.Inserir(nome, "Desc", valor);
        return s;
    }

    private static OrdemServico CriarOrdemServicoRecebida()
    {
        var os = new OrdemServico();
        os.Inserir(Guid.NewGuid(), Guid.NewGuid(),
            [new OrdemServicoServico(Guid.NewGuid(), Guid.NewGuid(), "Serviço", 100m)], []);
        return os;
    }

    private static OrdemServico CriarOrdemServicoEmDiagnostico()
    {
        var os = CriarOrdemServicoRecebida();
        os.IniciarDiagnostico();
        return os;
    }

    private static OrdemServico CriarOrdemServicoAguardandoAprovacao()
    {
        var os = CriarOrdemServicoEmDiagnostico();
        os.FinalizarDiagnostico();
        return os;
    }

    private static OrdemServico CriarOrdemServicoEmExecucao(params OrdemServicoServico[] extras)
    {
        var base_ = new OrdemServicoServico(Guid.NewGuid(), Guid.NewGuid(), "Base", 100m);
        var todos = new List<OrdemServicoServico> { base_ };
        todos.AddRange(extras);
        var os = new OrdemServico();
        os.Inserir(Guid.NewGuid(), Guid.NewGuid(), todos, []);
        os.IniciarDiagnostico();
        os.FinalizarDiagnostico();
        os.AprovarOrcamento();
        return os;
    }

    private static OrdemServico CriarOrdemServicoEmExecucaoUnico(OrdemServicoServico servico)
    {
        var os = new OrdemServico();
        os.Inserir(Guid.NewGuid(), Guid.NewGuid(), [servico], []);
        os.IniciarDiagnostico();
        os.FinalizarDiagnostico();
        os.AprovarOrcamento();
        return os;
    }

    private static OrdemServico CriarOrdemServicoFinalizada()
    {
        var servico = new OrdemServicoServico(Guid.NewGuid(), Guid.NewGuid(), "Serviço", 100m);
        var os      = CriarOrdemServicoEmExecucaoUnico(servico);
        servico.IniciarExecucao();
        os.FinalizarExecucaoServico(servico.Id);
        return os;
    }

    private static OrdemServicoOutput CriarOrdemServicoOutput() =>
        new(Guid.NewGuid(),
            new OrdemServicoClienteOutput(Guid.NewGuid(), "Cliente", "52998224725"),
            new OrdemServicoVeiculoOutput(Guid.NewGuid(), "ABC1234", "Honda", "Civic", AnoAtual),
            DateTime.UtcNow, null, null, "Recebida", 100m, [], []);
}

// ── Builders ─────────────────────────────────────────────────────────────────

file class InserirOrdemServicoBuilder
{
    private FakeOrdemServicoGateway          _os        = new();
    private FakeClienteGateway               _cliente   = new();
    private FakeServicoGateway               _servico   = new();
    private FakeProdutoGateway               _produto   = new();
    private FakeInserirOrdemServicoPresenter _presenter = new();

    public InserirOrdemServicoBuilder ComCliente(params Cliente[] c)  { _cliente = new FakeClienteGateway(c);  return this; }
    public InserirOrdemServicoBuilder ComServicos(params Servico[] s) { _servico = new FakeServicoGateway(s);  return this; }
    public InserirOrdemServicoBuilder ComProdutos(params Produto[] p) { _produto = new FakeProdutoGateway(p);  return this; }

    public (InserirOrdemServicoUseCase UseCase,
            FakeOrdemServicoGateway OS,
            FakeInserirOrdemServicoPresenter Presenter) Build()
    {
        var uc = new InserirOrdemServicoUseCase(_os, _cliente, _servico, _produto, _presenter);
        return (uc, _os, _presenter);
    }
}

file class InserirProdutosBuilder
{
    private FakeOrdemServicoGateway      _os        = new();
    private FakeProdutoGateway           _produto   = new();
    private FakeInserirProdutosPresenter _presenter = new();

    public InserirProdutosBuilder ComOS(OrdemServico os)             { _os      = new FakeOrdemServicoGateway(os); return this; }
    public InserirProdutosBuilder ComProdutos(params Produto[] pp)   { _produto = new FakeProdutoGateway(pp);      return this; }

    public (InserirProdutosUseCase UseCase,
            FakeOrdemServicoGateway OS,
            FakeInserirProdutosPresenter Presenter) Build()
    {
        var uc = new InserirProdutosUseCase(_os, _produto, _presenter);
        return (uc, _os, _presenter);
    }
}

file class InserirServicosBuilder
{
    private FakeOrdemServicoGateway      _os        = new();
    private FakeServicoGateway           _servico   = new();
    private FakeInserirServicosPresenter _presenter = new();

    public InserirServicosBuilder ComOS(OrdemServico os)             { _os      = new FakeOrdemServicoGateway(os); return this; }
    public InserirServicosBuilder ComServicos(params Servico[] ss)   { _servico = new FakeServicoGateway(ss);      return this; }

    public (InserirServicosUseCase UseCase,
            FakeOrdemServicoGateway OS,
            FakeInserirServicosPresenter Presenter) Build()
    {
        var uc = new InserirServicosUseCase(_os, _servico, _presenter);
        return (uc, _os, _presenter);
    }
}

/// <summary>
/// Builder genérico para use cases simples (gateway + presenter de estado).
/// Usado por: FinalizarDiagnostico, AprovarOrcamento, ReprovarOrcamento.
/// </summary>
file class EstadoUseCaseBuilder<TUseCase, TInput>
{
    private FakeOrdemServicoGateway _os        = new();
    private FakeEstadoPresenter     _presenter = new();

    public EstadoUseCaseBuilder<TUseCase, TInput> ComOS(OrdemServico os) { _os = new FakeOrdemServicoGateway(os); return this; }

    public (TUseCase UseCase, FakeEstadoPresenter Presenter) Build(
        Func<FakeOrdemServicoGateway, FakeEstadoPresenter, TUseCase> factory)
        => (factory(_os, _presenter), _presenter);
}

// ── Fakes ─────────────────────────────────────────────────────────────────────

file class FakeOrdemServicoGateway : IOrdemServicoGateway
{
    private readonly OrdemServico? _os;
    public bool SalvarFoiChamado    { get; private set; }
    public bool AtualizarFoiChamado { get; private set; }
    public bool RemoverFoiChamado   { get; private set; }
    public OrdemServico? OsSalva    { get; private set; }

    public FakeOrdemServicoGateway(OrdemServico? os = null) => _os = os;

    public Task<OrdemServico?> BuscarPorId(Guid id, CancellationToken ct)
        => Task.FromResult(_os?.Id == id ? _os : null);

    public Task<OrdemServico?> BuscarComServicos(Guid id, CancellationToken ct)
        => Task.FromResult(_os?.Id == id ? _os : null);

    public Task<OrdemServico?> BuscarComProdutos(Guid id, CancellationToken ct)
        => Task.FromResult(_os?.Id == id ? _os : null);

    public Task<OrdemServico?> BuscarComServicosProdutos(Guid id, CancellationToken ct)
        => Task.FromResult(_os?.Id == id ? _os : null);

    public Task Salvar(OrdemServico os, CancellationToken ct)
    {
        SalvarFoiChamado = true;
        OsSalva = os;
        return Task.CompletedTask;
    }

    public Task Atualizar(OrdemServico os, CancellationToken ct)
    {
        AtualizarFoiChamado = true;
        return Task.CompletedTask;
    }

    public Task Remover(OrdemServico os, CancellationToken ct)
    {
        RemoverFoiChamado = true;
        return Task.CompletedTask;
    }
}

file class FakeOrdemServicoQueryGateway : IOrdemServicoQueryGateway
{
    private readonly OrdemServicoOutput? _output;
    public FakeOrdemServicoQueryGateway(OrdemServicoOutput? output = null) => _output = output;

    public Task<OrdemServicoOutput?> BuscarComDetalhes(Guid id, CancellationToken ct)
        => Task.FromResult(_output?.Id == id ? _output : null);

    public Task<OrdemServicoStatusOutput?> BuscarStatus(Guid id, CancellationToken ct)
        => Task.FromResult(_output?.Id == id ? new OrdemServicoStatusOutput(id, _output.Status) : null);

    public Task<(IReadOnlyList<OrdemServicoOutput>, int)> BuscarPaginado(PagedRequest p, CancellationToken ct)
        => Task.FromResult(((IReadOnlyList<OrdemServicoOutput>)[], 0));

    public Task<(IReadOnlyList<OrdemServicoPorDocumentoOutput>, int)> BuscarPaginadoPorDocumento(string doc, PagedRequest p, CancellationToken ct)
        => Task.FromResult(((IReadOnlyList<OrdemServicoPorDocumentoOutput>)[], 0));
}

file class FakeClienteGateway : IClienteGateway
{
    private readonly List<Cliente> _clientes;
    public FakeClienteGateway(params Cliente[] clientes) => _clientes = [..clientes];

    public Task<Cliente?> BuscarPorId(Guid id, CancellationToken ct)
        => Task.FromResult(_clientes.FirstOrDefault(c => c.Id == id));

    public Task<Cliente?> BuscarComVeiculos(Guid id, CancellationToken ct)
        => Task.FromResult(_clientes.FirstOrDefault(c => c.Id == id));

    public Task<bool> ExisteComDocumento(string doc, CancellationToken ct) => Task.FromResult(false);
    public Task<(IReadOnlyList<Cliente>, int)> BuscarPaginado(PagedRequest p, CancellationToken ct)
        => Task.FromResult(((IReadOnlyList<Cliente>)_clientes, _clientes.Count));
    public Task Salvar(Cliente c, CancellationToken ct)   => Task.CompletedTask;
    public Task Atualizar(Cliente c, CancellationToken ct) => Task.CompletedTask;
    public Task Remover(Cliente c, CancellationToken ct)  => Task.CompletedTask;
}

// ── Fake Presenters ───────────────────────────────────────────────────────────

file class FakeBuscarOrdemServicoPresenter : IBuscarOrdemServicoOutputPort
{
    public bool NaoEncontradoChamado { get; private set; }
    public OrdemServicoOutput? Output { get; private set; }
    public void NaoEncontrado()               => NaoEncontradoChamado = true;
    public void Ok(OrdemServicoOutput output) => Output = output;
}

file class FakeBuscarStatusPresenter : IBuscarStatusOutputPort
{
    public bool NaoEncontradoChamado { get; private set; }
    public OrdemServicoStatusOutput? Output { get; private set; }
    public void NaoEncontrado()                     => NaoEncontradoChamado = true;
    public void Ok(OrdemServicoStatusOutput output) => Output = output;
}

file class FakeInserirOrdemServicoPresenter : IInserirOrdemServicoOutputPort
{
    public bool ClienteNaoEncontradoChamado { get; private set; }
    public bool VeiculoNaoPertenceChamado   { get; private set; }
    public bool OkChamado                   { get; private set; }
    public Guid IdCriado                    { get; private set; }
    public void ClienteNaoEncontrado()                   => ClienteNaoEncontradoChamado = true;
    public void VeiculoNaoPertenceAoCliente(string nome) => VeiculoNaoPertenceChamado = true;
    public void Ok(Guid id)                              { OkChamado = true; IdCriado = id; }
}

file class FakeEstadoPresenter :
    IIniciarDiagnosticoOutputPort,
    IFinalizarDiagnosticoOutputPort,
    IAprovarOrcamentoOutputPort,
    IReprovarOrcamentoOutputPort,
    IIniciarExecucaoServicoOutputPort,
    IFinalizarExecucaoServicoOutputPort,
    IEntregarOutputPort,
    IRemoverOrdemServicoOutputPort,
    IRemoverProdutoOrdemServicoOutputPort,
    IRemoverServicoOrdemServicoOutputPort
{
    public bool NaoEncontradoChamado { get; private set; }
    public bool OkChamado            { get; private set; }
    public void NaoEncontrado() => NaoEncontradoChamado = true;
    public void Ok()            => OkChamado = true;
}

file class FakeInserirProdutosPresenter : IInserirProdutosOutputPort
{
    public bool NaoEncontradoChamado       { get; private set; }
    public bool EstoqueInsuficienteChamado { get; private set; }
    public bool OkChamado                  { get; private set; }
    public void NaoEncontrado()                 => NaoEncontradoChamado = true;
    public void EstoqueInsuficiente(string msg) => EstoqueInsuficienteChamado = true;
    public void Ok()                            => OkChamado = true;
}

file class FakeInserirServicosPresenter : IInserirServicosOutputPort
{
    public bool NaoEncontradoChamado { get; private set; }
    public bool OkChamado            { get; private set; }
    public void NaoEncontrado() => NaoEncontradoChamado = true;
    public void Ok()            => OkChamado = true;
}