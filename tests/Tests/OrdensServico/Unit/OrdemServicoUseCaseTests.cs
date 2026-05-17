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
using Application.Servicos.DTOs;
using Domain.Clientes;
using Domain.Clientes.Gateways;
using Domain.Clientes.ValueObjects;
using Domain.Clientes.Veiculos;
using Domain.Clientes.Veiculos.ValueObjects;
using Domain.OrdensServico;
using Domain.OrdensServico.Enums;
using Domain.OrdensServico.Gateways;
using Domain.OrdensServico.Produtos;
using Domain.OrdensServico.Servicos;
using Domain.OrdensServico.Servicos.Enums;
using Domain.Produtos;
using Domain.Produtos.Gateways;
using Domain.Servicos;
using Domain.Servicos.Gateways;
using SharedKernel;
using Tests.Produtos.Unit;
using Tests.Servicos.Unit;
using Xunit;

namespace Tests.OrdensServico.Unit;

public class OrdemServicoUseCaseTests
{
    private static readonly int AnoAtual = DateTime.Now.Year;

    // ── BuscarOrdemServico ───────────────────────────────────────

    [Fact]
    public async Task Buscar_QuandoNaoExiste_ChamaNaoEncontrado()
    {
        var gateway   = new FakeOrdemServicoGateway();
        var query     = new FakeOrdemServicoQueryGateway();
        var presenter = new FakeBuscarOrdemServicoPresenter();
        var useCase   = new BuscarOrdemServicoUseCase(query, presenter);

        await useCase.Execute(new BuscarOrdemServicoInput(Guid.NewGuid()), CancellationToken.None);

        Assert.True(presenter.NaoEncontradoChamado);
    }

    [Fact]
    public async Task Buscar_QuandoExiste_ChamaOk()
    {
        var output    = CriarOrdemServicoOutput();
        var query     = new FakeOrdemServicoQueryGateway(output);
        var presenter = new FakeBuscarOrdemServicoPresenter();
        var useCase   = new BuscarOrdemServicoUseCase(query, presenter);

        await useCase.Execute(new BuscarOrdemServicoInput(output.Id), CancellationToken.None);

        Assert.False(presenter.NaoEncontradoChamado);
        Assert.Equal(output.Id, presenter.Output?.Id);
    }

    // ── BuscarStatus ─────────────────────────────────────────────

    [Fact]
    public async Task BuscarStatus_QuandoNaoExiste_ChamaNaoEncontrado()
    {
        var query     = new FakeOrdemServicoQueryGateway();
        var presenter = new FakeBuscarStatusPresenter();
        var useCase   = new BuscarStatusUseCase(query, presenter);

        await useCase.Execute(new BuscarStatusInput(Guid.NewGuid()), CancellationToken.None);

        Assert.True(presenter.NaoEncontradoChamado);
    }

    // ── Inserir ──────────────────────────────────────────────────

    [Fact]
    public async Task Inserir_QuandoClienteNaoExiste_ChamaClienteNaoEncontrado()
    {
        var osGateway      = new FakeOrdemServicoGateway();
        var clienteGateway = new FakeClienteGateway();
        var servicoGateway = new FakeServicoGateway();
        var produtoGateway = new FakeProdutoGateway();
        var presenter      = new FakeInserirOrdemServicoPresenter();
        var useCase        = new InserirOrdemServicoUseCase(osGateway, clienteGateway, servicoGateway, produtoGateway, presenter);

        await useCase.Execute(new InserirOrdemServicoInput(Guid.NewGuid(), Guid.NewGuid(), [], []), CancellationToken.None);

        Assert.True(presenter.ClienteNaoEncontradoChamado);
        Assert.False(osGateway.SalvarFoiChamado);
    }

    [Fact]
    public async Task Inserir_QuandoVeiculoNaoPertenceAoCliente_ChamaVeiculoNaoPertence()
    {
        var cliente        = CriarCliente();
        var clienteGateway = new FakeClienteGateway(cliente);
        var osGateway      = new FakeOrdemServicoGateway();
        var servicoGateway = new FakeServicoGateway();
        var produtoGateway = new FakeProdutoGateway();
        var presenter      = new FakeInserirOrdemServicoPresenter();
        var useCase        = new InserirOrdemServicoUseCase(osGateway, clienteGateway, servicoGateway, produtoGateway, presenter);

        await useCase.Execute(new InserirOrdemServicoInput(cliente.Id, Guid.NewGuid(), [], []), CancellationToken.None);

        Assert.True(presenter.VeiculoNaoPertenceChamado);
        Assert.False(osGateway.SalvarFoiChamado);
    }

    [Fact]
    public async Task Inserir_QuandoDadosValidos_ChamaOkEPersiste()
    {
        var cliente        = CriarClienteComVeiculo();
        var veiculo        = cliente.Veiculos[0];
        var clienteGateway = new FakeClienteGateway(cliente);
        var osGateway      = new FakeOrdemServicoGateway();
        var servicoGateway = new FakeServicoGateway();
        var produtoGateway = new FakeProdutoGateway();
        var presenter      = new FakeInserirOrdemServicoPresenter();
        var useCase        = new InserirOrdemServicoUseCase(osGateway, clienteGateway, servicoGateway, produtoGateway, presenter);

        await useCase.Execute(new InserirOrdemServicoInput(cliente.Id, veiculo.Id, [], []), CancellationToken.None);

        Assert.True(presenter.OkChamado);
        Assert.True(osGateway.SalvarFoiChamado);
    }

    // ── IniciarDiagnostico ───────────────────────────────────────

    [Fact]
    public async Task IniciarDiagnostico_QuandoNaoExiste_ChamaNaoEncontrado()
    {
        var gateway   = new FakeOrdemServicoGateway();
        var presenter = new FakeEstadoPresenter();
        var useCase   = new IniciarDiagnosticoUseCase(gateway, presenter);

        await useCase.Execute(new IniciarDiagnosticoInput(Guid.NewGuid()), CancellationToken.None);

        Assert.True(presenter.NaoEncontradoChamado);
        Assert.False(gateway.AtualizarFoiChamado);
    }

    [Fact]
    public async Task IniciarDiagnostico_QuandoRecebida_MudaStatusEChamaOk()
    {
        var os        = CriarOrdemServicoRecebida();
        var gateway   = new FakeOrdemServicoGateway(os);
        var presenter = new FakeEstadoPresenter();
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
        var gateway   = new FakeOrdemServicoGateway();
        var presenter = new FakeEstadoPresenter();
        var useCase   = new FinalizarDiagnosticoUseCase(gateway, presenter);

        await useCase.Execute(new FinalizarDiagnosticoInput(Guid.NewGuid()), CancellationToken.None);

        Assert.True(presenter.NaoEncontradoChamado);
    }

    [Fact]
    public async Task FinalizarDiagnostico_QuandoEmDiagnostico_MudaParaAguardandoAprovacao()
    {
        var os        = CriarOrdemServicoEmDiagnostico();
        Assert.NotEqual(Guid.Empty, os.Id);
        var gateway   = new FakeOrdemServicoGateway(os);
        var presenter = new FakeEstadoPresenter();
        var useCase   = new FinalizarDiagnosticoUseCase(gateway, presenter);

        await useCase.Execute(new FinalizarDiagnosticoInput(os.Id), CancellationToken.None);

        Assert.Equal(StatusOrdemServico.AguardandoAprovacao, os.Status);
        Assert.True(presenter.OkChamado);
    }

    // ── AprovarOrcamento ─────────────────────────────────────────

    [Fact]
    public async Task AprovarOrcamento_QuandoNaoExiste_ChamaNaoEncontrado()
    {
        var gateway   = new FakeOrdemServicoGateway();
        var presenter = new FakeEstadoPresenter();
        var useCase   = new AprovarOrcamentoUseCase(gateway, presenter);

        await useCase.Execute(new AprovarOrcamentoInput(Guid.NewGuid()), CancellationToken.None);

        Assert.True(presenter.NaoEncontradoChamado);
    }

    [Fact]
    public async Task AprovarOrcamento_QuandoAguardandoAprovacao_MudaParaEmExecucao()
    {
        var os        = CriarOrdemServicoAguardandoAprovacao();
        var gateway   = new FakeOrdemServicoGateway(os);
        var presenter = new FakeEstadoPresenter();
        var useCase   = new AprovarOrcamentoUseCase(gateway, presenter);

        await useCase.Execute(new AprovarOrcamentoInput(os.Id), CancellationToken.None);

        Assert.Equal(StatusOrdemServico.EmExecucao, os.Status);
        Assert.True(presenter.OkChamado);
    }

    // ── ReprovarOrcamento ────────────────────────────────────────

    [Fact]
    public async Task ReprovarOrcamento_QuandoAguardandoAprovacao_MudaParaReprovado()
    {
        var os        = CriarOrdemServicoAguardandoAprovacao();
        var gateway   = new FakeOrdemServicoGateway(os);
        var presenter = new FakeEstadoPresenter();
        var useCase   = new ReprovarOrcamentoUseCase(gateway, presenter);

        await useCase.Execute(new ReprovarOrcamentoInput(os.Id), CancellationToken.None);

        Assert.Equal(StatusOrdemServico.Finalizada, os.Status);
        Assert.True(presenter.OkChamado);
    }

    // ── IniciarExecucaoServico ───────────────────────────────────

    [Fact]
    public async Task IniciarExecucaoServico_QuandoNaoExiste_ChamaNaoEncontrado()
    {
        var gateway   = new FakeOrdemServicoGateway();
        var presenter = new FakeEstadoPresenter();
        var useCase   = new IniciarExecucaoServicoUseCase(gateway, presenter);

        await useCase.Execute(new IniciarExecucaoServicoInput(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.True(presenter.NaoEncontradoChamado);
    }

    [Fact]
    public async Task IniciarExecucaoServico_QuandoEmExecucao_IniciaServico()
    {
        var servico   = new OrdemServicoServico(Guid.NewGuid(), Guid.NewGuid(), "Serviço", 100m);
        var os        = CriarOrdemServicoEmExecucao(servico);
        var gateway   = new FakeOrdemServicoGateway(os);
        var presenter = new FakeEstadoPresenter();
        var useCase   = new IniciarExecucaoServicoUseCase(gateway, presenter);

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
        var gateway   = new FakeOrdemServicoGateway(os);
        var presenter = new FakeEstadoPresenter();
        var useCase   = new FinalizarExecucaoServicoUseCase(gateway, presenter);

        await useCase.Execute(new FinalizarExecucaoServicoInput(os.Id, servico.Id), CancellationToken.None);

        Assert.Equal(StatusOrdemServico.Finalizada, os.Status);
        Assert.True(presenter.OkChamado);
    }

    [Fact]
    public async Task FinalizarExecucaoServico_QuandoAindaHaServicoPendente_NaoFinaliza()
    {
        var s1        = new OrdemServicoServico(Guid.NewGuid(), Guid.NewGuid(), "S1", 100m);
        var s2        = new OrdemServicoServico(Guid.NewGuid(), Guid.NewGuid(), "S2", 100m);
        var os        = CriarOrdemServicoEmExecucao(s1, s2);
        s1.IniciarExecucao();
        var gateway   = new FakeOrdemServicoGateway(os);
        var presenter = new FakeEstadoPresenter();
        var useCase   = new FinalizarExecucaoServicoUseCase(gateway, presenter);

        await useCase.Execute(new FinalizarExecucaoServicoInput(os.Id, s1.Id), CancellationToken.None);

        Assert.Equal(StatusOrdemServico.EmExecucao, os.Status);
    }

    // ── Entregar ─────────────────────────────────────────────────

    [Fact]
    public async Task Entregar_QuandoFinalizada_MudaParaEntregue()
    {
        var os        = CriarOrdemServicoFinalizada();
        var gateway   = new FakeOrdemServicoGateway(os);
        var presenter = new FakeEstadoPresenter();
        var useCase   = new EntregarUseCase(gateway, presenter);

        await useCase.Execute(new EntregarInput(os.Id), CancellationToken.None);

        Assert.Equal(StatusOrdemServico.Entregue, os.Status);
        Assert.True(presenter.OkChamado);
    }

    // ── InserirProdutos ──────────────────────────────────────────

    [Fact]
    public async Task InserirProdutos_QuandoNaoExiste_ChamaNaoEncontrado()
    {
        var gateway        = new FakeOrdemServicoGateway();
        var produtoGateway = new FakeProdutoGateway();
        var presenter      = new FakeInserirProdutosPresenter();
        var useCase        = new InserirProdutosUseCase(gateway, produtoGateway, presenter);

        await useCase.Execute(new InserirProdutosInput(Guid.NewGuid(), []), CancellationToken.None);

        Assert.True(presenter.NaoEncontradoChamado);
    }

    [Fact]
    public async Task InserirProdutos_QuandoEstoqueInsuficiente_ChamaEstoqueInsuficiente()
    {
        var os             = CriarOrdemServicoEmDiagnostico();
        var produto        = CriarProduto(estoque: 2);
        var gateway        = new FakeOrdemServicoGateway(os);
        var produtoGateway = new FakeProdutoGateway(produto);
        var presenter      = new FakeInserirProdutosPresenter();
        var useCase        = new InserirProdutosUseCase(gateway, produtoGateway, presenter);

        await useCase.Execute(new InserirProdutosInput(os.Id,
            [new InserirProdutosItemInput(produto.Id, 10)]), CancellationToken.None);

        Assert.True(presenter.EstoqueInsuficienteChamado);
    }

    [Fact]
    public async Task InserirProdutos_QuandoDadosValidos_ChamaOk()
    {
        var os             = CriarOrdemServicoEmDiagnostico();
        var produto        = CriarProduto(estoque: 10);
        var gateway        = new FakeOrdemServicoGateway(os);
        var produtoGateway = new FakeProdutoGateway(produto);
        var presenter      = new FakeInserirProdutosPresenter();
        var useCase        = new InserirProdutosUseCase(gateway, produtoGateway, presenter);

        await useCase.Execute(new InserirProdutosInput(os.Id,
            [new InserirProdutosItemInput(produto.Id, 3)]), CancellationToken.None);

        Assert.True(presenter.OkChamado);
        Assert.True(gateway.AtualizarFoiChamado);
    }

    // ── InserirServicos ──────────────────────────────────────────

    [Fact]
    public async Task InserirServicos_QuandoNaoExiste_ChamaNaoEncontrado()
    {
        var gateway        = new FakeOrdemServicoGateway();
        var servicoGateway = new FakeServicoGateway();
        var presenter      = new FakeInserirServicosPresenter();
        var useCase        = new InserirServicosUseCase(gateway, servicoGateway, presenter);

        await useCase.Execute(new InserirServicosOrdemServicoInput(Guid.NewGuid(), []), CancellationToken.None);

        Assert.True(presenter.NaoEncontradoChamado);
    }

    // ── Remover ──────────────────────────────────────────────────

    [Fact]
    public async Task Remover_QuandoNaoExiste_ChamaNaoEncontrado()
    {
        var gateway   = new FakeOrdemServicoGateway();
        var presenter = new FakeEstadoPresenter();
        var useCase   = new RemoverOrdemServicoUseCase(gateway, presenter);

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
        var gateway   = new FakeOrdemServicoGateway();
        var presenter = new FakeEstadoPresenter();
        var useCase   = new RemoverProdutoUseCase(gateway, presenter);

        await useCase.Execute(new RemoverProdutoInput(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.True(presenter.NaoEncontradoChamado);
    }

    [Fact]
    public async Task RemoverProduto_QuandoExiste_RemoveEChamaOk()
    {
        var os         = CriarOrdemServicoEmDiagnostico();
        var produtoOs  = new OrdemServicoProduto(os.Id, Guid.NewGuid(), "Produto", 50m, 1);
        os.Produtos.Add(produtoOs);
        var gateway    = new FakeOrdemServicoGateway(os);
        var presenter  = new FakeEstadoPresenter();
        var useCase    = new RemoverProdutoUseCase(gateway, presenter);

        await useCase.Execute(new RemoverProdutoInput(os.Id, produtoOs.Id), CancellationToken.None);

        Assert.True(presenter.OkChamado);
        Assert.Empty(os.Produtos);
    }

    // ── RemoverServico ───────────────────────────────────────────

    [Fact]
    public async Task RemoverServico_QuandoNaoExiste_ChamaNaoEncontrado()
    {
        var gateway   = new FakeOrdemServicoGateway();
        var presenter = new FakeEstadoPresenter();
        var useCase   = new RemoverServicoUseCase(gateway, presenter);

        await useCase.Execute(new RemoverServicoOrdemServicoInput(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.True(presenter.NaoEncontradoChamado);
    }

    [Fact]
    public async Task RemoverServico_QuandoExiste_RemoveEChamaOk()
    {
        var os         = CriarOrdemServicoEmDiagnostico();
        var servicoOs  = new OrdemServicoServico(os.Id, Guid.NewGuid(), "Serviço Extra", 200m);
        os.Servicos.Add(servicoOs);
        var gateway    = new FakeOrdemServicoGateway(os);
        var presenter  = new FakeEstadoPresenter();
        var useCase    = new RemoverServicoUseCase(gateway, presenter);

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

// ── Fakes ────────────────────────────────────────────────────────────────────

file class FakeOrdemServicoGateway : IOrdemServicoGateway
{
    private readonly OrdemServico? _os;
    public bool SalvarFoiChamado    { get; private set; }
    public bool AtualizarFoiChamado { get; private set; }
    public bool RemoverFoiChamado   { get; private set; }

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
    public Task Salvar(Cliente c, CancellationToken ct) => Task.CompletedTask;
    public Task Atualizar(Cliente c, CancellationToken ct) => Task.CompletedTask;
    public Task Remover(Cliente c, CancellationToken ct) => Task.CompletedTask;
}

// ── Fake Presenters ──────────────────────────────────────────────────────────

file class FakeBuscarOrdemServicoPresenter : IBuscarOrdemServicoOutputPort
{
    public bool NaoEncontradoChamado { get; private set; }
    public OrdemServicoOutput? Output { get; private set; }
    public void NaoEncontrado() => NaoEncontradoChamado = true;
    public void Ok(OrdemServicoOutput output) => Output = output;
}

file class FakeBuscarStatusPresenter : IBuscarStatusOutputPort
{
    public bool NaoEncontradoChamado { get; private set; }
    public OrdemServicoStatusOutput? Output { get; private set; }
    public void NaoEncontrado() => NaoEncontradoChamado = true;
    public void Ok(OrdemServicoStatusOutput output) => Output = output;
}

file class FakeInserirOrdemServicoPresenter : IInserirOrdemServicoOutputPort
{
    public bool ClienteNaoEncontradoChamado { get; private set; }
    public bool VeiculoNaoPertenceChamado   { get; private set; }
    public bool OkChamado                   { get; private set; }
    public void ClienteNaoEncontrado()                    => ClienteNaoEncontradoChamado = true;
    public void VeiculoNaoPertenceAoCliente(string nome)  => VeiculoNaoPertenceChamado = true;
    public void Ok(Guid id)                               => OkChamado = true;
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
    public bool NaoEncontradoChamado        { get; private set; }
    public bool EstoqueInsuficienteChamado  { get; private set; }
    public bool OkChamado                   { get; private set; }
    public void NaoEncontrado()                    => NaoEncontradoChamado = true;
    public void EstoqueInsuficiente(string msg)    => EstoqueInsuficienteChamado = true;
    public void Ok()                               => OkChamado = true;
}

file class FakeInserirServicosPresenter : IInserirServicosOutputPort
{
    public bool NaoEncontradoChamado { get; private set; }
    public bool OkChamado            { get; private set; }
    public void NaoEncontrado() => NaoEncontradoChamado = true;
    public void Ok()            => OkChamado = true;
}