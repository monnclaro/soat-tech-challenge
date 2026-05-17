using Application.Clientes.UseCases;
using Application.Clientes.UseCases.AtualizarCliente;
using Application.Clientes.UseCases.BuscarCliente;
using Application.Clientes.UseCases.BuscarListaPaginada;
using Application.Clientes.UseCases.InserirCliente;
using Application.Clientes.UseCases.RemoverCliente;
using Domain.Clientes;
using Domain.Clientes.Gateways;
using Domain.Clientes.ValueObjects;
using SharedKernel;
using SharedKernel.Exceptions;
using Xunit;

namespace Tests.Clientes.Unit;

public class ClienteUseCaseTests
{
    private const string CpfValido  = "52998224725";
    private const string CnpjValido = "11222333000181";

    // ── Buscar ───────────────────────────────────────────────────

    [Fact]
    public async Task Buscar_QuandoNaoExiste_ChamaNaoEncontrado()
    {
        var gateway   = new FakeClienteGateway();
        var presenter = new FakeBuscarClientePresenter();
        var useCase   = new BuscarClienteUseCase(gateway, presenter);

        await useCase.Execute(new BuscarClienteInput(Guid.NewGuid()), CancellationToken.None);

        Assert.True(presenter.NaoEncontradoChamado);
    }

    [Fact]
    public async Task Buscar_QuandoExiste_RetornaDadosCorretos()
    {
        var cliente   = CriarCliente("Maria Souza");
        var gateway   = new FakeClienteGateway(cliente);
        var presenter = new FakeBuscarClientePresenter();
        var useCase   = new BuscarClienteUseCase(gateway, presenter);

        await useCase.Execute(new BuscarClienteInput(cliente.Id), CancellationToken.None);

        Assert.False(presenter.NaoEncontradoChamado);
        Assert.Equal("Maria Souza", presenter.Output?.Nome);
        Assert.Equal(CpfValido, presenter.Output?.Documento);
    }

    // ── BuscarListaPaginada ──────────────────────────────────────

    [Fact]
    public async Task BuscarListaPaginada_QuandoSemClientes_RetornaVazio()
    {
        var gateway   = new FakeClienteGateway();
        var presenter = new FakeBuscarListaPaginadaPresenter();
        var useCase   = new BuscarListaPaginadaClienteUseCase(gateway, presenter);

        await useCase.Execute(new BuscarListaPaginadaClienteInput(new PagedRequest(1, 10)), CancellationToken.None);

        Assert.Equal(0, presenter.Output?.TotalCount);
        Assert.Empty(presenter.Output?.Items ?? []);
    }

    [Fact]
    public async Task BuscarListaPaginada_RetornaTotalEItemsCorretos()
    {
        var gateway = new FakeClienteGateway(
            CriarCliente("A", "52998224725"),
            CriarCliente("B", "11144477735"),
            CriarCliente("C", "11222333000181"));
        var presenter = new FakeBuscarListaPaginadaPresenter();
        var useCase   = new BuscarListaPaginadaClienteUseCase(gateway, presenter);

        await useCase.Execute(new BuscarListaPaginadaClienteInput(new PagedRequest(1, 10)), CancellationToken.None);

        Assert.Equal(3, presenter.Output?.TotalCount);
        Assert.Equal(3, presenter.Output?.Items.Count);
    }

    #region Inserir

    [Fact]
    public async Task Inserir_QuandoCpfValido_ChamaOkEPersiste()
    {
        var gateway   = new FakeClienteGateway();
        var presenter = new FakeInserirClientePresenter();
        var useCase   = new InserirClienteUseCase(gateway, presenter); // sem DocumentoService

        await useCase.Execute(new InserirClienteInput("João Silva", CpfValido), CancellationToken.None);

        Assert.NotNull(presenter.Output);
        Assert.Equal("João Silva", presenter.Output!.Nome);
        Assert.Equal(CpfValido, presenter.Output.Documento);
        Assert.True(gateway.SalvarFoiChamado);
    }

    [Fact]
    public async Task Inserir_QuandoCpfJaCadastrado_ChamaDocumentoDuplicado()
    {
        var cliente   = CriarCliente("Existente");
        var gateway   = new FakeClienteGateway(existeDocumento: true, cliente);
        var presenter = new FakeInserirClientePresenter();
        var useCase   = new InserirClienteUseCase(gateway, presenter);

        await useCase.Execute(new InserirClienteInput("Novo", CpfValido), CancellationToken.None);

        Assert.True(presenter.DocumentoDuplicadoChamado);
        Assert.False(gateway.SalvarFoiChamado);
    }

    [Theory]
    [InlineData("11111111111")]
    [InlineData("12345678901")]
    [InlineData("abc")]
    public async Task Inserir_QuandoDocumentoInvalido_LancaDomainException(string documento)
    {
        var gateway   = new FakeClienteGateway();
        var presenter = new FakeInserirClientePresenter();
        var useCase   = new InserirClienteUseCase(gateway, presenter);

        await Assert.ThrowsAsync<DomainException>(() =>
            useCase.Execute(new InserirClienteInput("Nome", documento), CancellationToken.None));

        Assert.False(gateway.SalvarFoiChamado);
    }

    [Fact]
    public async Task Inserir_QuandoCnpjValido_ChamaOk()
    {
        var gateway   = new FakeClienteGateway();
        var presenter = new FakeInserirClientePresenter();
        var useCase   = new InserirClienteUseCase(gateway, presenter);

        await useCase.Execute(new InserirClienteInput("Empresa X", CnpjValido), CancellationToken.None);

        Assert.Equal(CnpjValido, presenter.Output?.Documento);
    }
    #endregion

    #region Atualização

    [Fact]
    public async Task Atualizar_QuandoNaoExiste_ChamaNaoEncontrado()
    {
        var gateway   = new FakeClienteGateway();
        var presenter = new FakeAtualizarClientePresenter();
        var useCase   = new AtualizarClienteUseCase(gateway, presenter);

        await useCase.Execute(new AtualizarClienteInput(Guid.NewGuid(), "Nome"), CancellationToken.None);

        Assert.True(presenter.NaoEncontradoChamado);
        Assert.False(gateway.AtualizarFoiChamado);
    }

    [Fact]
    public async Task Atualizar_QuandoExiste_AlteraNomeEChamaOk()
    {
        var cliente   = CriarCliente("Nome Antigo");
        var gateway   = new FakeClienteGateway(cliente);
        var presenter = new FakeAtualizarClientePresenter();
        var useCase   = new AtualizarClienteUseCase(gateway, presenter);

        await useCase.Execute(new AtualizarClienteInput(cliente.Id, "Nome Novo"), CancellationToken.None);

        Assert.Equal("Nome Novo", presenter.Output?.Nome);
        Assert.True(gateway.AtualizarFoiChamado);
    }

    [Fact]
    public async Task Atualizar_NaoAlteraDocumento()
    {
        var cliente   = CriarCliente();
        var gateway   = new FakeClienteGateway(cliente);
        var presenter = new FakeAtualizarClientePresenter();
        var useCase   = new AtualizarClienteUseCase(gateway, presenter);

        await useCase.Execute(new AtualizarClienteInput(cliente.Id, "Novo Nome"), CancellationToken.None);

        Assert.Equal(CpfValido, presenter.Output?.Documento);
    }
    #endregion

    #region Remoção

    [Fact]
    public async Task Remover_QuandoNaoExiste_ChamaNaoEncontrado()
    {
        var gateway   = new FakeClienteGateway();
        var presenter = new FakeRemoverClientePresenter();
        var useCase   = new RemoverClienteUseCase(gateway, presenter);

        await useCase.Execute(new RemoverClienteInput(Guid.NewGuid()), CancellationToken.None);

        Assert.True(presenter.NaoEncontradoChamado);
        Assert.False(gateway.RemoverFoiChamado);
    }

    [Fact]
    public async Task Remover_QuandoExiste_RemoveEChamaOk()
    {
        var cliente   = CriarCliente();
        var gateway   = new FakeClienteGateway(cliente);
        var presenter = new FakeRemoverClientePresenter();
        var useCase   = new RemoverClienteUseCase(gateway, presenter);

        await useCase.Execute(new RemoverClienteInput(cliente.Id), CancellationToken.None);

        Assert.True(presenter.OkChamado);
        Assert.True(gateway.RemoverFoiChamado);
    }
    #endregion

    #region Helpers

    private static Cliente CriarCliente(
        string nome = "Cliente Teste",
        string documento = "52998224725")
    {
        var c = new Cliente();
        c.Inserir(nome, DocumentoCliente.Criar(documento));
        return c;
    }
    #endregion
}

file class FakeClienteGateway : IClienteGateway
{
    private readonly List<Cliente> _clientes;
    private readonly bool _existeDocumento;
    public bool SalvarFoiChamado    { get; private set; }
    public bool AtualizarFoiChamado { get; private set; }
    public bool RemoverFoiChamado   { get; private set; }

    public FakeClienteGateway(bool existeDocumento = false, params Cliente[] clientes)
    {
        _clientes        = [..clientes];
        _existeDocumento = existeDocumento;
    }

    public FakeClienteGateway(params Cliente[] clientes) : this(false, clientes) { }

    public Task<Cliente?> BuscarPorId(Guid id, CancellationToken ct)
        => Task.FromResult(_clientes.FirstOrDefault(c => c.Id == id));

    public Task<Cliente?> BuscarComVeiculos(Guid id, CancellationToken ct)
        => Task.FromResult(_clientes.FirstOrDefault(c => c.Id == id));

    public Task<bool> ExisteComDocumento(string documento, CancellationToken ct)
        => Task.FromResult(_existeDocumento);

    public Task<(IReadOnlyList<Cliente> Items, int Total)> BuscarPaginado(PagedRequest p, CancellationToken ct)
    {
        var items = _clientes.Skip((p.Pagina - 1) * p.Tamanho).Take(p.Tamanho).ToList();
        return Task.FromResult(((IReadOnlyList<Cliente>)items, _clientes.Count));
    }

    public Task Salvar(Cliente cliente, CancellationToken ct)
    {
        SalvarFoiChamado = true;
        _clientes.Add(cliente);
        return Task.CompletedTask;
    }

    public Task Atualizar(Cliente cliente, CancellationToken ct)
    {
        AtualizarFoiChamado = true;
        return Task.CompletedTask;
    }

    public Task Remover(Cliente cliente, CancellationToken ct)
    {
        RemoverFoiChamado = true;
        _clientes.Remove(cliente);
        return Task.CompletedTask;
    }
}

file class FakeBuscarClientePresenter : IBuscarClienteOutputPort
{
    public bool NaoEncontradoChamado { get; private set; }
    public ClienteOutput? Output { get; private set; }
    public void NaoEncontrado() => NaoEncontradoChamado = true;
    public void Ok(ClienteOutput output) => Output = output;
}

file class FakeBuscarListaPaginadaPresenter : IBuscarListaPaginadaClienteOutputPort
{
    public PagedResult<ClienteOutput>? Output { get; private set; }
    public void Ok(PagedResult<ClienteOutput> resultado) => Output = resultado;
}

file class FakeInserirClientePresenter : IInserirClienteOutputPort
{
    public bool DocumentoDuplicadoChamado { get; private set; }
    public ClienteOutput? Output { get; private set; }
    public void DocumentoDuplicado(string mensagem) => DocumentoDuplicadoChamado = true;
    public void Ok(ClienteOutput output) => Output = output;
}

file class FakeAtualizarClientePresenter : IAtualizarClienteOutputPort
{
    public bool NaoEncontradoChamado { get; private set; }
    public ClienteOutput? Output { get; private set; }
    public void NaoEncontrado() => NaoEncontradoChamado = true;
    public void Ok(ClienteOutput output) => Output = output;
}

file class FakeRemoverClientePresenter : IRemoverClienteOutputPort
{
    public bool NaoEncontradoChamado { get; private set; }
    public bool OkChamado { get; private set; }
    public void NaoEncontrado() => NaoEncontradoChamado = true;
    public void Ok() => OkChamado = true;
}