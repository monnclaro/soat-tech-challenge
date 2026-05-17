using Application.Produtos.DTOs;
using Application.Produtos.UseCases.AtualizarProduto;
using Application.Produtos.UseCases.BuscarListaPaginada;
using Application.Produtos.UseCases.BuscarProduto;
using Application.Produtos.UseCases.DecrementarEstoque;
using Application.Produtos.UseCases.IncrementarEstoque;
using Application.Produtos.UseCases.InserirProduto;
using Application.Produtos.UseCases.RemoverProduto;
using Domain.Produtos;
using Domain.Produtos.Gateways;
using SharedKernel;
using SharedKernel.Exceptions;
using Xunit;

namespace Tests.Produtos.Unit;

public class ProdutoUseCaseTests
{
    // ── Buscar ───────────────────────────────────────────────────

    [Fact]
    public async Task Buscar_QuandoNaoExiste_ChamaNaoEncontrado()
    {
        var gateway   = new FakeProdutoGateway();
        var presenter = new FakeBuscarProdutoPresenter();
        var useCase   = new BuscarProdutoUseCase(gateway, presenter);

        await useCase.Execute(new BuscarProdutoInput(Guid.NewGuid()), CancellationToken.None);

        Assert.True(presenter.NaoEncontradoChamado);
    }

    [Fact]
    public async Task Buscar_QuandoExiste_RetornaDadosCorretos()
    {
        var produto   = CriarProduto("Monitor 4K", 3500m, 8);
        var gateway   = new FakeProdutoGateway(produto);
        var presenter = new FakeBuscarProdutoPresenter();
        var useCase   = new BuscarProdutoUseCase(gateway, presenter);

        await useCase.Execute(new BuscarProdutoInput(produto.Id), CancellationToken.None);

        Assert.False(presenter.NaoEncontradoChamado);
        Assert.Equal("Monitor 4K", presenter.Output?.Nome);
        Assert.Equal(3500m, presenter.Output?.Valor);
        Assert.Equal(8, presenter.Output?.QuantidadeEmEstoque);
    }

    // ── BuscarListaPaginada ──────────────────────────────────────

    [Fact]
    public async Task BuscarListaPaginada_QuandoSemProdutos_RetornaVazio()
    {
        var gateway   = new FakeProdutoGateway();
        var presenter = new FakeBuscarListaPaginadaPresenter();
        var useCase   = new BuscarListaPaginadaProdutoUseCase(gateway, presenter);

        await useCase.Execute(new BuscarListaPaginadaInput(new PagedRequest(1, 10)), CancellationToken.None);

        Assert.Equal(0, presenter.Output?.TotalCount);
        Assert.Empty(presenter.Output?.Items ?? []);
    }

    [Fact]
    public async Task BuscarListaPaginada_RetornaTotalEItemsCorretos()
    {
        var gateway = new FakeProdutoGateway(
            CriarProduto("Zebra"),
            CriarProduto("Abacate"),
            CriarProduto("Manga"));
        var presenter = new FakeBuscarListaPaginadaPresenter();
        var useCase   = new BuscarListaPaginadaProdutoUseCase(gateway, presenter);

        await useCase.Execute(new BuscarListaPaginadaInput(new PagedRequest(1, 10)), CancellationToken.None);

        Assert.Equal(3, presenter.Output?.TotalCount);
        Assert.Equal(3, presenter.Output?.Items.Count);
    }

    // ── Inserir ──────────────────────────────────────────────────

    [Fact]
    public async Task Inserir_QuandoDadosValidos_ChamaOkEPersiste()
    {
        var gateway   = new FakeProdutoGateway();
        var presenter = new FakeInserirProdutoPresenter();
        var useCase   = new InserirProdutoUseCase(gateway, presenter);

        await useCase.Execute(new InserirProdutoInput("Teclado", "Mecânico", 350m, 20), CancellationToken.None);

        Assert.NotNull(presenter.Output);
        Assert.Equal("Teclado", presenter.Output!.Nome);
        Assert.Equal(350m, presenter.Output.Valor);
        Assert.True(gateway.SalvarFoiChamado);
    }

    [Fact]
    public async Task Inserir_QuandoValorInvalido_LancaDomainException()
    {
        var gateway   = new FakeProdutoGateway();
        var presenter = new FakeInserirProdutoPresenter();
        var useCase   = new InserirProdutoUseCase(gateway, presenter);

        await Assert.ThrowsAsync<DomainException>(() =>
            useCase.Execute(new InserirProdutoInput("Produto", "desc", -1m, 5), CancellationToken.None));

        Assert.False(gateway.SalvarFoiChamado);
    }

    // ── Atualizar ────────────────────────────────────────────────

    [Fact]
    public async Task Atualizar_QuandoNaoExiste_ChamaNaoEncontrado()
    {
        var gateway   = new FakeProdutoGateway();
        var presenter = new FakeAtualizarProdutoPresenter();
        var useCase   = new AtualizarProdutoUseCase(gateway, presenter);

        await useCase.Execute(new AtualizarProdutoInput(Guid.NewGuid(), "Nome", "desc", 100m), CancellationToken.None);

        Assert.True(presenter.NaoEncontradoChamado);
        Assert.False(gateway.AtualizarFoiChamado);
    }

    [Fact]
    public async Task Atualizar_QuandoExiste_AtualizaEChamaOk()
    {
        var produto   = CriarProduto("Antigo", 100m);
        var gateway   = new FakeProdutoGateway(produto);
        var presenter = new FakeAtualizarProdutoPresenter();
        var useCase   = new AtualizarProdutoUseCase(gateway, presenter);

        await useCase.Execute(new AtualizarProdutoInput(produto.Id, "Novo", "Nova desc", 200m), CancellationToken.None);

        Assert.Equal("Novo", presenter.Output?.Nome);
        Assert.Equal(200m, presenter.Output?.Valor);
        Assert.True(gateway.AtualizarFoiChamado);
    }

    [Fact]
    public async Task Atualizar_QuandoValorInvalido_LancaDomainException()
    {
        var produto   = CriarProduto();
        var gateway   = new FakeProdutoGateway(produto);
        var presenter = new FakeAtualizarProdutoPresenter();
        var useCase   = new AtualizarProdutoUseCase(gateway, presenter);

        await Assert.ThrowsAsync<DomainException>(() =>
            useCase.Execute(new AtualizarProdutoInput(produto.Id, "Nome", "desc", -20m), CancellationToken.None));

        Assert.False(gateway.AtualizarFoiChamado);
    }

    // ── IncrementarEstoque ───────────────────────────────────────

    [Fact]
    public async Task IncrementarEstoque_QuandoNaoExiste_ChamaNaoEncontrado()
    {
        var gateway   = new FakeProdutoGateway();
        var presenter = new FakeIncrementarEstoquePresenter();
        var useCase   = new IncrementarEstoqueUseCase(gateway, presenter);

        await useCase.Execute(new IncrementarEstoqueInput(Guid.NewGuid(), 5), CancellationToken.None);

        Assert.True(presenter.NaoEncontradoChamado);
        Assert.False(gateway.AtualizarFoiChamado);
    }

    [Fact]
    public async Task IncrementarEstoque_QuandoExiste_AtualizaEstoque()
    {
        var produto   = CriarProduto(estoque: 10);
        var gateway   = new FakeProdutoGateway(produto);
        var presenter = new FakeIncrementarEstoquePresenter();
        var useCase   = new IncrementarEstoqueUseCase(gateway, presenter);

        await useCase.Execute(new IncrementarEstoqueInput(produto.Id, 5), CancellationToken.None);

        Assert.Equal(15, presenter.Output?.QuantidadeEmEstoque);
        Assert.True(gateway.AtualizarFoiChamado);
    }

    [Fact]
    public async Task IncrementarEstoque_QuandoQuantidadeInvalida_LancaDomainException()
    {
        var produto   = CriarProduto();
        var gateway   = new FakeProdutoGateway(produto);
        var presenter = new FakeIncrementarEstoquePresenter();
        var useCase   = new IncrementarEstoqueUseCase(gateway, presenter);

        await Assert.ThrowsAsync<DomainException>(() =>
            useCase.Execute(new IncrementarEstoqueInput(produto.Id, 0), CancellationToken.None));

        Assert.False(gateway.AtualizarFoiChamado);
    }

    // ── DecrementarEstoque ───────────────────────────────────────

    [Fact]
    public async Task DecrementarEstoque_QuandoExiste_AtualizaEstoque()
    {
        var produto   = CriarProduto(estoque: 10);
        var gateway   = new FakeProdutoGateway(produto);
        var presenter = new FakeDecrementarEstoquePresenter();
        var useCase   = new DecrementarEstoqueUseCase(gateway, presenter);

        await useCase.Execute(new DecrementarEstoqueInput(
            [new DecrementarEstoqueItem(produto.Id, 5)]), CancellationToken.None);

        Assert.True(presenter.OkChamado);
        Assert.True(gateway.AtualizarLoteFoiChamado);
    }

    [Fact]
    public async Task DecrementarEstoque_QuandoQuantidadeInvalida_LancaDomainException()
    {
        var produto   = CriarProduto(estoque: 10);
        var gateway   = new FakeProdutoGateway(produto);
        var presenter = new FakeDecrementarEstoquePresenter();
        var useCase   = new DecrementarEstoqueUseCase(gateway, presenter);

        await Assert.ThrowsAsync<DomainException>(() =>
            useCase.Execute(new DecrementarEstoqueInput(
                [new DecrementarEstoqueItem(produto.Id, -6)]), CancellationToken.None));

        Assert.False(gateway.AtualizarLoteFoiChamado);
    }

    // ── Remover ──────────────────────────────────────────────────

    [Fact]
    public async Task Remover_QuandoNaoExiste_ChamaOkSemRemover()
    {
        var gateway   = new FakeProdutoGateway();
        var presenter = new FakeRemoverProdutoPresenter();
        var useCase   = new RemoverProdutoUseCase(gateway, presenter);

        await useCase.Execute(new RemoverProdutoInput(Guid.NewGuid()), CancellationToken.None);

        Assert.True(presenter.OkChamado);
        Assert.False(gateway.RemoverFoiChamado);
    }

    [Fact]
    public async Task Remover_QuandoExiste_RemoveEChamaOk()
    {
        var produto   = CriarProduto();
        var gateway   = new FakeProdutoGateway(produto);
        var presenter = new FakeRemoverProdutoPresenter();
        var useCase   = new RemoverProdutoUseCase(gateway, presenter);

        await useCase.Execute(new RemoverProdutoInput(produto.Id), CancellationToken.None);

        Assert.True(presenter.OkChamado);
        Assert.True(gateway.RemoverFoiChamado);
    }

    // ── Helpers ──────────────────────────────────────────────────

    private static Produto CriarProduto(
        string nome = "Produto Teste",
        decimal valor = 100m,
        int estoque = 10)
    {
        var p = new Produto();
        p.Inserir(nome, "Descrição", valor, estoque);
        return p;
    }
}

// ── Fakes ────────────────────────────────────────────────────────────────────

public class FakeProdutoGateway : IProdutoGateway
{
    private readonly List<Produto> _produtos;
    public bool SalvarFoiChamado       { get; private set; }
    public bool AtualizarFoiChamado    { get; private set; }
    public bool AtualizarLoteFoiChamado { get; private set; }
    public bool RemoverFoiChamado      { get; private set; }

    public FakeProdutoGateway(params Produto[] produtos) => _produtos = [..produtos];

    public Task<Produto?> BuscarPorId(Guid id, CancellationToken ct)
        => Task.FromResult(_produtos.FirstOrDefault(p => p.Id == id));

    public Task<IReadOnlyList<Produto>> BuscarPorIds(IReadOnlyList<Guid> ids, CancellationToken ct)
        => Task.FromResult<IReadOnlyList<Produto>>(_produtos.Where(p => ids.Contains(p.Id)).ToList());

    public Task<Dictionary<Guid, Produto>> BuscarDicionarioPorIds(IReadOnlyList<Guid> ids, CancellationToken ct)
        => Task.FromResult(_produtos.Where(p => ids.Contains(p.Id)).ToDictionary(p => p.Id));

    public Task<(IReadOnlyList<Produto> Items, int Total)> BuscarPaginado(string? filtro, PagedRequest p, CancellationToken ct)
    {
        var items = _produtos.Skip((p.Pagina - 1) * p.Tamanho).Take(p.Tamanho).ToList();
        return Task.FromResult(((IReadOnlyList<Produto>)items, _produtos.Count));
    }

    public Task Salvar(Produto produto, CancellationToken ct)
    {
        SalvarFoiChamado = true;
        _produtos.Add(produto);
        return Task.CompletedTask;
    }

    public Task Atualizar(Produto produto, CancellationToken ct)
    {
        AtualizarFoiChamado = true;
        return Task.CompletedTask;
    }

    public Task AtualizarLote(IReadOnlyList<Produto> produtos, CancellationToken ct)
    {
        AtualizarLoteFoiChamado = true;
        return Task.CompletedTask;
    }

    public Task Remover(Produto produto, CancellationToken ct)
    {
        RemoverFoiChamado = true;
        _produtos.Remove(produto);
        return Task.CompletedTask;
    }
}

file class FakeBuscarProdutoPresenter : IBuscarProdutoOutputPort
{
    public bool NaoEncontradoChamado { get; private set; }
    public ProdutoOutput? Output { get; private set; }
    public void NaoEncontrado() => NaoEncontradoChamado = true;
    public void Ok(ProdutoOutput output) => Output = output;
}

file class FakeBuscarListaPaginadaPresenter : IBuscarListaPaginadaProdutoOutputPort
{
    public PagedResult<ProdutoOutput>? Output { get; private set; }
    public void Ok(PagedResult<ProdutoOutput> resultado) => Output = resultado;
}

file class FakeInserirProdutoPresenter : IInserirProdutoOutputPort
{
    public ProdutoOutput? Output { get; private set; }
    public void Ok(ProdutoOutput output) => Output = output;
}

file class FakeAtualizarProdutoPresenter : IAtualizarProdutoOutputPort
{
    public bool NaoEncontradoChamado { get; private set; }
    public ProdutoOutput? Output { get; private set; }
    public void NaoEncontrado() => NaoEncontradoChamado = true;
    public void Ok(ProdutoOutput output) => Output = output;
}

file class FakeIncrementarEstoquePresenter : IIncrementarEstoqueOutputPort
{
    public bool NaoEncontradoChamado { get; private set; }
    public ProdutoOutput? Output { get; private set; }
    public void NaoEncontrado() => NaoEncontradoChamado = true;
    public void Ok(ProdutoOutput output) => Output = output;
}

file class FakeDecrementarEstoquePresenter : IDecrementarEstoqueOutputPort
{
    public bool OkChamado { get; private set; }
    public void Ok() => OkChamado = true;
}

file class FakeRemoverProdutoPresenter : IRemoverProdutoOutputPort
{
    public bool OkChamado { get; private set; }
    public void Ok() => OkChamado = true;
}