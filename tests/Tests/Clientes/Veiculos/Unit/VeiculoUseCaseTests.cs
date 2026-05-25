using Application.Clientes.Veiculos.UseCases;
using Application.Clientes.Veiculos.UseCases.AtualizarVeiculo;
using Application.Clientes.Veiculos.UseCases.BuscarListaPaginada;
using Application.Clientes.Veiculos.UseCases.BuscarVeiculo;
using Application.Clientes.Veiculos.UseCases.InserirVeiculo;
using Application.Clientes.Veiculos.UseCases.RemoverVeiculo;
using Domain.Clientes;
using Domain.Clientes.Gateways;
using Domain.Clientes.ValueObjects;
using Domain.Clientes.Veiculos;
using Domain.Clientes.Veiculos.Gateways;
using Domain.Clientes.Veiculos.ValueObjects;
using Domain.Common.Exceptions;
using SharedKernel.DTOs;

namespace Tests.Clientes.Veiculos.Unit;

public class VeiculoUseCaseTests
{
    private static readonly int AnoAtual = DateTime.Now.Year;

    // ── Buscar ───────────────────────────────────────────────────

    [Fact]
    public async Task Buscar_QuandoNaoExiste_ChamaNaoEncontrado()
    {
        var gateway   = new FakeVeiculoGateway();
        var presenter = new FakeBuscarVeiculoPresenter();
        var useCase   = new BuscarVeiculoUseCase(gateway, presenter);

        await useCase.Execute(new BuscarVeiculoInput(Guid.NewGuid()), CancellationToken.None);

        Assert.True(presenter.NaoEncontradoChamado);
    }

    [Fact]
    public async Task Buscar_QuandoExiste_RetornaDadosCorretos()
    {
        var veiculo   = CriarVeiculo("ABC1234");
        var gateway   = new FakeVeiculoGateway(veiculo);
        var presenter = new FakeBuscarVeiculoPresenter();
        var useCase   = new BuscarVeiculoUseCase(gateway, presenter);

        await useCase.Execute(new BuscarVeiculoInput(veiculo.Id), CancellationToken.None);

        Assert.False(presenter.NaoEncontradoChamado);
        Assert.Equal("ABC1234", presenter.Output?.Placa);
        Assert.Equal(veiculo.IdCliente, presenter.Output?.IdCliente);
    }

    // ── BuscarListaPaginada ──────────────────────────────────────

    [Fact]
    public async Task BuscarListaPaginada_QuandoSemVeiculos_RetornaVazio()
    {
        var gateway   = new FakeVeiculoGateway();
        var presenter = new FakeBuscarListaPaginadaPresenter();
        var useCase   = new BuscarListaPaginadaVeiculoUseCase(gateway, presenter);

        await useCase.Execute(new BuscarListaPaginadaVeiculoInput(Guid.NewGuid(), new PagedRequest(1, 10)), CancellationToken.None);

        Assert.Equal(0, presenter.Output?.TotalCount);
        Assert.Empty(presenter.Output?.Items ?? []);
    }

    [Fact]
    public async Task BuscarListaPaginada_FiltraPorIdCliente()
    {
        var idCliente    = Guid.NewGuid();
        var outroCliente = Guid.NewGuid();
        var gateway = new FakeVeiculoGateway(
            CriarVeiculo("AAA1111", idCliente),
            CriarVeiculo("AAA2222", idCliente),
            CriarVeiculo("BBB1111", outroCliente));
        var presenter = new FakeBuscarListaPaginadaPresenter();
        var useCase   = new BuscarListaPaginadaVeiculoUseCase(gateway, presenter);

        await useCase.Execute(new BuscarListaPaginadaVeiculoInput(idCliente, new PagedRequest(1, 10)), CancellationToken.None);

        Assert.Equal(2, presenter.Output?.TotalCount);
        Assert.All(presenter.Output!.Items, r => Assert.Equal(idCliente, r.IdCliente));
    }

    // ── Inserir ──────────────────────────────────────────────────

    [Fact]
    public async Task Inserir_QuandoClienteNaoExiste_ChamaClienteNaoEncontrado()
    {
        var veiculoGateway  = new FakeVeiculoGateway();
        var clienteGateway  = new FakeClienteGateway();
        var presenter       = new FakeInserirVeiculoPresenter();
        var useCase         = new InserirVeiculoUseCase(veiculoGateway, clienteGateway, presenter);

        await useCase.Execute(new InserirVeiculoInput(Guid.NewGuid(), "ABC1234", "Honda", "Civic", AnoAtual), CancellationToken.None);

        Assert.True(presenter.ClienteNaoEncontradoChamado);
        Assert.False(veiculoGateway.InserirFoiChamado);
    }

    [Fact]
    public async Task Inserir_QuandoPlacaDuplicada_ChamaPlacaDuplicada()
    {
        var cliente        = CriarCliente();
        var veiculoGateway = new FakeVeiculoGateway(placaEmUso: true);
        var clienteGateway = new FakeClienteGateway(cliente);
        var presenter      = new FakeInserirVeiculoPresenter();
        var useCase        = new InserirVeiculoUseCase(veiculoGateway, clienteGateway, presenter);

        await useCase.Execute(new InserirVeiculoInput(cliente.Id, "ABC1234", "Honda", "Civic", AnoAtual), CancellationToken.None);

        Assert.True(presenter.PlacaDuplicadaChamado);
        Assert.False(veiculoGateway.InserirFoiChamado);
    }

    [Fact]
    public async Task Inserir_QuandoDadosValidos_ChamaOkEPersiste()
    {
        var cliente        = CriarCliente();
        var veiculoGateway = new FakeVeiculoGateway();
        var clienteGateway = new FakeClienteGateway(cliente);
        var presenter      = new FakeInserirVeiculoPresenter();
        var useCase        = new InserirVeiculoUseCase(veiculoGateway, clienteGateway, presenter);

        await useCase.Execute(new InserirVeiculoInput(cliente.Id, "ABC1234", "Honda", "Civic", AnoAtual), CancellationToken.None);

        Assert.NotNull(presenter.Output);
        Assert.Equal("ABC1234", presenter.Output!.Placa);
        Assert.Equal(cliente.Id, presenter.Output.IdCliente);
        Assert.True(veiculoGateway.InserirFoiChamado);
    }

    [Theory]
    [InlineData("INVALIDA")]
    [InlineData("")]
    [InlineData("12345")]
    public async Task Inserir_QuandoPlacaInvalida_LancaDomainException(string placa)
    {
        var cliente        = CriarCliente();
        var veiculoGateway = new FakeVeiculoGateway();
        var clienteGateway = new FakeClienteGateway(cliente);
        var presenter      = new FakeInserirVeiculoPresenter();
        var useCase        = new InserirVeiculoUseCase(veiculoGateway, clienteGateway, presenter);

        await Assert.ThrowsAsync<DomainException>(() =>
            useCase.Execute(new InserirVeiculoInput(cliente.Id, placa, "Honda", "Civic", AnoAtual), CancellationToken.None));

        Assert.False(veiculoGateway.InserirFoiChamado);
    }

    // ── Atualizar ────────────────────────────────────────────────

    [Fact]
    public async Task Atualizar_QuandoNaoExiste_ChamaNaoEncontrado()
    {
        var gateway   = new FakeVeiculoGateway();
        var presenter = new FakeAtualizarVeiculoPresenter();
        var useCase   = new AtualizarVeiculoUseCase(gateway, presenter);

        await useCase.Execute(new AtualizarVeiculoInput(Guid.NewGuid(), "ABC1234", "Honda", "Civic", AnoAtual), CancellationToken.None);

        Assert.True(presenter.NaoEncontradoChamado);
        Assert.False(gateway.AtualizarFoiChamado);
    }

    [Fact]
    public async Task Atualizar_QuandoPlacaDuplicada_ChamaPlacaDuplicada()
    {
        var veiculo   = CriarVeiculo("ABC1234");
        var gateway   = new FakeVeiculoGateway(placaEmUsoExcetoId: true, veiculos: veiculo);
        var presenter = new FakeAtualizarVeiculoPresenter();
        var useCase   = new AtualizarVeiculoUseCase(gateway, presenter);

        await useCase.Execute(new AtualizarVeiculoInput(veiculo.Id, "XYZ9W87", "Honda", "Civic", AnoAtual), CancellationToken.None);

        Assert.True(presenter.PlacaDuplicadaChamado);
        Assert.False(gateway.AtualizarFoiChamado);
    }

    [Fact]
    public async Task Atualizar_QuandoExiste_AtualizaEChamaOk()
    {
        var veiculo   = CriarVeiculo("ABC1234");
        var gateway   = new FakeVeiculoGateway(veiculo);
        var presenter = new FakeAtualizarVeiculoPresenter();
        var useCase   = new AtualizarVeiculoUseCase(gateway, presenter);

        await useCase.Execute(new AtualizarVeiculoInput(veiculo.Id, "XYZ9W87", "Toyota", "Corolla", AnoAtual - 1), CancellationToken.None);

        Assert.Equal("XYZ9W87", presenter.Output?.Placa);
        Assert.Equal("Toyota", presenter.Output?.Marca);
        Assert.True(gateway.AtualizarFoiChamado);
    }

    // ── Remover ──────────────────────────────────────────────────

    [Fact]
    public async Task Remover_QuandoNaoExiste_ChamaNaoEncontrado()
    {
        var gateway   = new FakeVeiculoGateway();
        var presenter = new FakeRemoverVeiculoPresenter();
        var useCase   = new RemoverVeiculoUseCase(gateway, presenter);

        await useCase.Execute(new RemoverVeiculoInput(Guid.NewGuid()), CancellationToken.None);

        Assert.True(presenter.NaoEncontradoChamado);
        Assert.False(gateway.RemoverFoiChamado);
    }

    [Fact]
    public async Task Remover_QuandoExiste_RemoveEChamaOk()
    {
        var veiculo   = CriarVeiculo();
        var gateway   = new FakeVeiculoGateway(veiculo);
        var presenter = new FakeRemoverVeiculoPresenter();
        var useCase   = new RemoverVeiculoUseCase(gateway, presenter);

        await useCase.Execute(new RemoverVeiculoInput(veiculo.Id), CancellationToken.None);

        Assert.True(presenter.OkChamado);
        Assert.True(gateway.RemoverFoiChamado);
    }

    // ── Helpers ──────────────────────────────────────────────────

    private static Veiculo CriarVeiculo(string placa = "ABC1D23", Guid? idCliente = null)
    {
        var v = new Veiculo();
        v.Inserir(idCliente ?? Guid.NewGuid(), Placa.Criar(placa), "Honda", "Civic", AnoAtual);
        return v;
    }

    private static Cliente CriarCliente()
    {
        var c = new Cliente();
        c.Inserir("Cliente Teste", DocumentoCliente.Criar("52998224725"));
        return c;
    }
}

// ── Fakes ────────────────────────────────────────────────────────────────────

file class FakeVeiculoGateway : IVeiculoGateway
{
    private readonly List<Veiculo> _veiculos;
    private readonly bool _placaEmUso;
    private readonly bool _placaEmUsoExcetoId;
    public bool InserirFoiChamado  { get; private set; }
    public bool AtualizarFoiChamado { get; private set; }
    public bool RemoverFoiChamado  { get; private set; }

    public FakeVeiculoGateway(
        bool placaEmUso = false,
        bool placaEmUsoExcetoId = false,
        params Veiculo[] veiculos)
    {
        _veiculos           = [..veiculos];
        _placaEmUso         = placaEmUso;
        _placaEmUsoExcetoId = placaEmUsoExcetoId;
    }

    public FakeVeiculoGateway(params Veiculo[] veiculos) : this(false, false, veiculos) { }

    public Task<Veiculo?> BuscarPorId(Guid id, CancellationToken ct)
        => Task.FromResult(_veiculos.FirstOrDefault(v => v.Id == id));

    public Task<bool> ExisteComPlaca(string placa, CancellationToken ct)
        => Task.FromResult(_placaEmUso);

    public Task<bool> ExisteComPlacaExcetoId(string placa, Guid idVeiculo, CancellationToken ct)
        => Task.FromResult(_placaEmUsoExcetoId);

    public Task<(IReadOnlyList<Veiculo> Items, int Total)> BuscarPaginadoPorCliente(
        Guid idCliente, PagedRequest p, CancellationToken ct)
    {
        var filtered = _veiculos.Where(v => v.IdCliente == idCliente).ToList();
        var items    = filtered.Skip((p.Pagina - 1) * p.Tamanho).Take(p.Tamanho).ToList();
        return Task.FromResult(((IReadOnlyList<Veiculo>)items, filtered.Count));
    }

    public Task Inserir(Veiculo veiculo, CancellationToken ct)
    {
        InserirFoiChamado = true;
        _veiculos.Add(veiculo);
        return Task.CompletedTask;
    }

    public Task Salvar(Veiculo veiculo, CancellationToken ct)
    {
        InserirFoiChamado = true;
        _veiculos.Add(veiculo);
        return Task.CompletedTask;
    }

    public Task Atualizar(Veiculo veiculo, CancellationToken ct)
    {
        AtualizarFoiChamado = true;
        return Task.CompletedTask;
    }

    public Task Remover(Veiculo veiculo, CancellationToken ct)
    {
        RemoverFoiChamado = true;
        _veiculos.Remove(veiculo);
        return Task.CompletedTask;
    }
}

file class FakeClienteGateway : IClienteGateway
{
    private readonly List<Cliente> _clientes;

    public FakeClienteGateway(params Cliente[] clientes) => _clientes = [..clientes];

    public Task<Cliente?> BuscarPorId(Guid id, CancellationToken ct)
        => Task.FromResult(_clientes.FirstOrDefault(c => c.Id == id));

    public Task<Cliente?> BuscarComVeiculos(Guid id, CancellationToken ct)
        => Task.FromResult(_clientes.FirstOrDefault(c => c.Id == id));

    public Task<bool> ExisteComDocumento(string documento, CancellationToken ct)
        => Task.FromResult(false);

    public Task<(IReadOnlyList<Cliente> Items, int Total)> BuscarPaginado(PagedRequest p, CancellationToken ct)
        => Task.FromResult(((IReadOnlyList<Cliente>)_clientes, _clientes.Count));

    public Task Salvar(Cliente cliente, CancellationToken ct) => Task.CompletedTask;
    public Task Atualizar(Cliente cliente, CancellationToken ct) => Task.CompletedTask;
    public Task Remover(Cliente cliente, CancellationToken ct) => Task.CompletedTask;
}

file class FakeBuscarVeiculoPresenter : IBuscarVeiculoOutputPort
{
    public bool NaoEncontradoChamado { get; private set; }
    public VeiculoOutput? Output { get; private set; }
    public void NaoEncontrado() => NaoEncontradoChamado = true;
    public void Ok(VeiculoOutput output) => Output = output;
}

file class FakeBuscarListaPaginadaPresenter : IBuscarListaPaginadaVeiculoOutputPort
{
    public PagedResult<VeiculoOutput>? Output { get; private set; }
    public void Ok(PagedResult<VeiculoOutput> resultado) => Output = resultado;
}

file class FakeInserirVeiculoPresenter : IInserirVeiculoOutputPort
{
    public bool ClienteNaoEncontradoChamado { get; private set; }
    public bool PlacaDuplicadaChamado       { get; private set; }
    public VeiculoOutput? Output            { get; private set; }
    public void ClienteNaoEncontrado()          => ClienteNaoEncontradoChamado = true;
    public void PlacaDuplicada(string mensagem) => PlacaDuplicadaChamado = true;
    public void Ok(VeiculoOutput output)        => Output = output;
}

file class FakeAtualizarVeiculoPresenter : IAtualizarVeiculoOutputPort
{
    public bool NaoEncontradoChamado  { get; private set; }
    public bool PlacaDuplicadaChamado { get; private set; }
    public VeiculoOutput? Output      { get; private set; }
    public void NaoEncontrado()             => NaoEncontradoChamado = true;
    public void PlacaDuplicada(string msg)  => PlacaDuplicadaChamado = true;
    public void Ok(VeiculoOutput output)    => Output = output;
}

file class FakeRemoverVeiculoPresenter : IRemoverVeiculoOutputPort
{
    public bool NaoEncontradoChamado { get; private set; }
    public bool OkChamado            { get; private set; }
    public void NaoEncontrado() => NaoEncontradoChamado = true;
    public void Ok()            => OkChamado = true;
}