using Application.Servicos.DTOs;
using Application.Servicos.Queries;
using Application.Servicos.Queries.BuscarListaPaginada;
using Application.Servicos.Queries.BuscarServico;
using Application.Servicos.Queries.BuscarTempoMedioExecucao;
using Application.Servicos.UseCases.AtualizarServico;
using Application.Servicos.UseCases.InserirServico;
using Application.Servicos.UseCases.RemoverServico;
using Domain.Servicos;
using Domain.Servicos.Gateways;
using SharedKernel;
using SharedKernel.Exceptions;
using Xunit;

namespace Tests.Servicos.Unit;

public class ServicoUseCaseTests
{
    // ── Buscar ───────────────────────────────────────────────────

    [Fact]
    public async Task Buscar_QuandoNaoExiste_ChamaNaoEncontrado()
    {
        var gateway   = new FakeServicoGateway();
        var presenter = new FakeBuscarServicoPresenter();
        var useCase   = new BuscarServicoUseCase(gateway, presenter);

        await useCase.Execute(new BuscarServicoInput(Guid.NewGuid()), CancellationToken.None);

        Assert.True(presenter.NaoEncontradoChamado);
    }

    [Fact]
    public async Task Buscar_QuandoExiste_ChamaOkComDadosCorretos()
    {
        var servico   = CriarServico("Alinhamento", 150m);
        var gateway   = new FakeServicoGateway(servico);
        var presenter = new FakeBuscarServicoPresenter();
        var useCase   = new BuscarServicoUseCase(gateway, presenter);

        await useCase.Execute(new BuscarServicoInput(servico.Id), CancellationToken.None);

        Assert.False(presenter.NaoEncontradoChamado);
        Assert.Equal("Alinhamento", presenter.Output?.Nome);
        Assert.Equal(150m, presenter.Output?.Valor);
    }

    // ── BuscarListaPaginada ──────────────────────────────────────

    [Fact]
    public async Task BuscarListaPaginada_QuandoSemServicos_RetornaVazio()
    {
        var queryGateway = new FakeServicoQueryGateway();
        var presenter = new FakeBuscarListaPaginadaPresenter();
        var useCase = new BuscarListaPaginadaUseCase(queryGateway, presenter);

        await useCase.Execute(new BuscarListaPaginadaInput(new PagedRequest(1, 10)), CancellationToken.None);

        Assert.Equal(0, presenter.Output?.TotalCount);
        Assert.Empty(presenter.Output?.Items ?? []);
    }

    [Fact]
    public async Task BuscarListaPaginada_RetornaTotalEItemsCorretos()
    {
        var queryGateway = new FakeServicoQueryGateway(
            null,
            CriarServico("Zebra"),
            CriarServico("Abacate"),
            CriarServico("Manga"));

        var presenter = new FakeBuscarListaPaginadaPresenter();
        var useCase   = new BuscarListaPaginadaUseCase(queryGateway, presenter);

        await useCase.Execute(new BuscarListaPaginadaInput(new PagedRequest(1, 10)), CancellationToken.None);

        Assert.Equal(3, presenter.Output?.TotalCount);
        Assert.Equal(3, presenter.Output?.Items.Count);
    }

    // ── BuscarTempoMedioExecucao ─────────────────────────────────

    [Fact]
    public async Task BuscarTempoMedioExecucao_QuandoSemDados_RetornaVazio()
    {
        var queryGateway = new FakeServicoQueryGateway();
        var presenter    = new FakeBuscarTempoMedioPresenter();
        var useCase      = new BuscarTempoMedioExecucaoUseCase(queryGateway, presenter);

        await useCase.Execute(CancellationToken.None);

        Assert.Empty(presenter.Output ?? []);
    }

    [Fact]
    public async Task BuscarTempoMedioExecucao_QuandoComDados_RetornaDados()
    {
        var output = new TempoMedioExecucaoOutput("Alinhamento", 60.0, 30.0, 90.0);
        var queryGateway = new FakeServicoQueryGateway(tempoMedio: [output]);
        var presenter    = new FakeBuscarTempoMedioPresenter();
        var useCase      = new BuscarTempoMedioExecucaoUseCase(queryGateway, presenter);

        await useCase.Execute(CancellationToken.None);

        Assert.Single(presenter.Output!);
        Assert.Equal("Alinhamento", presenter.Output![0].Servico);
        Assert.Equal(60.0, presenter.Output[0].TempoMedioMinutos);
    }

    // ── Inserir ──────────────────────────────────────────────────

    [Fact]
    public async Task Inserir_QuandoDadosValidos_ChamaOkEPersiste()
    {
        var gateway   = new FakeServicoGateway();
        var presenter = new FakeInserirServicoPresenter();
        var useCase   = new InserirServicoUseCase(gateway, presenter);

        await useCase.Execute(new InserirServicoInput("Troca de Óleo", "Desc", 250m), CancellationToken.None);

        Assert.NotNull(presenter.Output);
        Assert.Equal("Troca de Óleo", presenter.Output!.Nome);
        Assert.Equal(250m, presenter.Output.Valor);
        Assert.True(gateway.SalvarFoiChamado);
    }

    [Fact]
    public async Task Inserir_QuandoNomeInvalido_LancaDomainException()
    {
        var gateway   = new FakeServicoGateway();
        var presenter = new FakeInserirServicoPresenter();
        var useCase   = new InserirServicoUseCase(gateway, presenter);

        await Assert.ThrowsAsync<DomainException>(() =>
            useCase.Execute(new InserirServicoInput("", "desc", 100m), CancellationToken.None));

        Assert.False(gateway.SalvarFoiChamado);
    }

    [Fact]
    public async Task Inserir_QuandoValorInvalido_LancaDomainException()
    {
        var gateway   = new FakeServicoGateway();
        var presenter = new FakeInserirServicoPresenter();
        var useCase   = new InserirServicoUseCase(gateway, presenter);

        await Assert.ThrowsAsync<DomainException>(() =>
            useCase.Execute(new InserirServicoInput("Nome", "desc", -10m), CancellationToken.None));

        Assert.False(gateway.SalvarFoiChamado);
    }

    // ── Atualizar ────────────────────────────────────────────────

    [Fact]
    public async Task Atualizar_QuandoNaoExiste_ChamaNaoEncontrado()
    {
        var gateway   = new FakeServicoGateway();
        var presenter = new FakeAtualizarServicoPresenter();
        var useCase   = new AtualizarServicoUseCase(gateway, presenter);

        await useCase.Execute(new AtualizarServicoInput(Guid.NewGuid(), "Nome", "desc", 100m), CancellationToken.None);

        Assert.True(presenter.NaoEncontradoChamado);
        Assert.False(gateway.AtualizarFoiChamado);
    }

    [Fact]
    public async Task Atualizar_QuandoExiste_AtualizaEChamaOk()
    {
        var servico   = CriarServico("Antigo", 100m);
        var gateway   = new FakeServicoGateway(servico);
        var presenter = new FakeAtualizarServicoPresenter();
        var useCase   = new AtualizarServicoUseCase(gateway, presenter);

        await useCase.Execute(new AtualizarServicoInput(servico.Id, "Novo", "Nova desc", 300m), CancellationToken.None);

        Assert.Equal("Novo", presenter.Output?.Nome);
        Assert.Equal(300m, presenter.Output?.Valor);
        Assert.True(gateway.AtualizarFoiChamado);
    }

    [Fact]
    public async Task Atualizar_QuandoValorInvalido_LancaDomainException()
    {
        var servico   = CriarServico();
        var gateway   = new FakeServicoGateway(servico);
        var presenter = new FakeAtualizarServicoPresenter();
        var useCase   = new AtualizarServicoUseCase(gateway, presenter);

        await Assert.ThrowsAsync<DomainException>(() =>
            useCase.Execute(new AtualizarServicoInput(servico.Id, "Nome", "desc", -1m), CancellationToken.None));

        Assert.False(gateway.AtualizarFoiChamado);
    }

    // ── Remover ──────────────────────────────────────────────────

    [Fact]
    public async Task Remover_QuandoNaoExiste_ChamaOkSemRemover()
    {
        var gateway   = new FakeServicoGateway();
        var presenter = new FakeRemoverServicoPresenter();
        var useCase   = new RemoverServicoUseCase(gateway, presenter);

        await useCase.Execute(new RemoverServicoInput(Guid.NewGuid()), CancellationToken.None);

        Assert.True(presenter.OkChamado);
        Assert.False(gateway.RemoverFoiChamado);
    }

    [Fact]
    public async Task Remover_QuandoExiste_RemoveEChamaOk()
    {
        var servico   = CriarServico();
        var gateway   = new FakeServicoGateway(servico);
        var presenter = new FakeRemoverServicoPresenter();
        var useCase   = new RemoverServicoUseCase(gateway, presenter);

        await useCase.Execute(new RemoverServicoInput(servico.Id), CancellationToken.None);

        Assert.True(presenter.OkChamado);
        Assert.True(gateway.RemoverFoiChamado);
    }

    // ── Helpers ──────────────────────────────────────────────────

    private static Servico CriarServico(string nome = "Serviço Teste", decimal valor = 100m)
    {
        var s = new Servico();
        s.Inserir(nome, "Descrição", valor);
        return s;
    }
}

// ── Fake IServicoGateway (escrita) ───────────────────────────────────────────

public class FakeServicoGateway : IServicoGateway
{
    private readonly List<Servico> _servicos;
    public bool SalvarFoiChamado    { get; private set; }
    public bool AtualizarFoiChamado { get; private set; }
    public bool RemoverFoiChamado   { get; private set; }

    public FakeServicoGateway(params Servico[] servicos) => _servicos = [..servicos];

    public Task<Servico?> BuscarPorId(Guid id, CancellationToken ct)
        => Task.FromResult(_servicos.FirstOrDefault(s => s.Id == id));

    public Task<Dictionary<Guid, Servico>> BuscarPorIds(IReadOnlyList<Guid> ids, CancellationToken ct)
        => Task.FromResult(_servicos.Where(s => ids.Contains(s.Id)).ToDictionary(s => s.Id));

    public Task Salvar(Servico servico, CancellationToken ct)
    {
        SalvarFoiChamado = true;
        _servicos.Add(servico);
        return Task.CompletedTask;
    }

    public Task Atualizar(Servico servico, CancellationToken ct)
    {
        AtualizarFoiChamado = true;
        return Task.CompletedTask;
    }

    public Task Remover(Servico servico, CancellationToken ct)
    {
        RemoverFoiChamado = true;
        _servicos.Remove(servico);
        return Task.CompletedTask;
    }
}

// ── Fake IServicoQueryGateway (leitura) ──────────────────────────────────────

file class FakeServicoQueryGateway : IServicoQueryGateway
{
    private readonly List<Servico> _servicos;
    private readonly IReadOnlyList<TempoMedioExecucaoOutput> _tempoMedio;

    public FakeServicoQueryGateway(
        IReadOnlyList<TempoMedioExecucaoOutput>? tempoMedio = null,
        params Servico[] servicos)
    {
        _servicos   = [..servicos];
        _tempoMedio = tempoMedio ?? [];
    }

    public Task<(IReadOnlyList<Servico> Items, int Total)> BuscarPaginado(
        string? filtro, PagedRequest p, CancellationToken ct)
    {
        var items = _servicos
            .Skip((p.Pagina - 1) * p.Tamanho)
            .Take(p.Tamanho)
            .ToList();

        return Task.FromResult(((IReadOnlyList<Servico>)items, _servicos.Count));
    }

    public Task<IReadOnlyList<TempoMedioExecucaoOutput>> BuscarTempoMedioExecucao(CancellationToken ct)
        => Task.FromResult(_tempoMedio);
}

// ── Fake Presenters ──────────────────────────────────────────────────────────

file class FakeBuscarServicoPresenter : IBuscarServicoOutputPort
{
    public bool NaoEncontradoChamado { get; private set; }
    public ServicoOutput? Output { get; private set; }
    public void NaoEncontrado() => NaoEncontradoChamado = true;
    public void Ok(ServicoOutput output) => Output = output;
}

file class FakeBuscarListaPaginadaPresenter : IBuscarListaPaginadaOutputPort
{
    public PagedResult<ServicoOutput>? Output { get; private set; }
    public void Ok(PagedResult<ServicoOutput> resultado) => Output = resultado;
}

file class FakeBuscarTempoMedioPresenter : IBuscarTempoMedioExecucaoOutputPort
{
    public IReadOnlyList<TempoMedioExecucaoOutput>? Output { get; private set; }
    public void Ok(IReadOnlyList<TempoMedioExecucaoOutput> resultado) => Output = resultado;
}

file class FakeInserirServicoPresenter : IInserirServicoOutputPort
{
    public ServicoOutput? Output { get; private set; }
    public void Ok(ServicoOutput output) => Output = output;
}

file class FakeAtualizarServicoPresenter : IAtualizarServicoOutputPort
{
    public bool NaoEncontradoChamado { get; private set; }
    public ServicoOutput? Output { get; private set; }
    public void NaoEncontrado() => NaoEncontradoChamado = true;
    public void Ok(ServicoOutput output) => Output = output;
}

file class FakeRemoverServicoPresenter : IRemoverServicoOutputPort
{
    public bool OkChamado { get; private set; }
    public void Ok() => OkChamado = true;
}