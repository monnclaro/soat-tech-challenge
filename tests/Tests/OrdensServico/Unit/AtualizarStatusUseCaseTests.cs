using Application.OrdensServico.UseCases.AtualizarStatus;
using Application.OrdensServico.UseCases.AtualizarStatus.DTOs;
using Domain.OrdensServico;
using Domain.OrdensServico.Enums;
using Domain.OrdensServico.Gateways;
using Domain.OrdensServico.Servicos;
using Xunit;

namespace Tests.OrdensServico.Unit;

public class AtualizarStatusUseCaseTests
{
    [Fact]
    public async Task Execute_QuandoNaoExiste_ChamaNaoEncontrado()
    {
        var gateway   = new FakeOrdemServicoGateway();
        var presenter = new FakeAtualizarStatusPresenter();
        var useCase   = new AtualizarStatusUseCase(gateway, presenter);

        await useCase.Execute(
            new AtualizarStatusOrdemServicoInput(Guid.NewGuid(), StatusOrdemServico.EmDiagnostico),
            CancellationToken.None);

        Assert.True(presenter.NaoEncontradoChamado);
        Assert.False(gateway.AtualizarFoiChamado);
    }

    [Theory]
    [InlineData(StatusOrdemServico.Recebida)]
    [InlineData(StatusOrdemServico.EmDiagnostico)]
    [InlineData(StatusOrdemServico.AguardandoAprovacao)]
    [InlineData(StatusOrdemServico.EmExecucao)]
    [InlineData(StatusOrdemServico.Finalizada)]
    [InlineData(StatusOrdemServico.Entregue)]
    public async Task Execute_QuandoExiste_AtualizaStatusEChamaOk(StatusOrdemServico novoStatus)
    {
        var os        = CriarOrdemServico();
        var gateway   = new FakeOrdemServicoGateway(os);
        var presenter = new FakeAtualizarStatusPresenter();
        var useCase   = new AtualizarStatusUseCase(gateway, presenter);

        await useCase.Execute(
            new AtualizarStatusOrdemServicoInput(os.Id, novoStatus),
            CancellationToken.None);

        Assert.Equal(novoStatus, os.Status);
        Assert.True(presenter.OkChamado);
        Assert.True(gateway.AtualizarFoiChamado);
    }

    [Fact]
    public async Task Execute_QuandoExiste_PersisteMudancaDeStatus()
    {
        var os        = CriarOrdemServico();
        var gateway   = new FakeOrdemServicoGateway(os);
        var presenter = new FakeAtualizarStatusPresenter();
        var useCase   = new AtualizarStatusUseCase(gateway, presenter);

        Assert.Equal(StatusOrdemServico.Recebida, os.Status);

        await useCase.Execute(
            new AtualizarStatusOrdemServicoInput(os.Id, StatusOrdemServico.EmDiagnostico),
            CancellationToken.None);

        Assert.Equal(StatusOrdemServico.EmDiagnostico, os.Status);
        Assert.True(gateway.AtualizarFoiChamado);
    }

    // ── Helpers ──────────────────────────────────────────────────

    private static OrdemServico CriarOrdemServico()
    {
        var os = new OrdemServico();
        os.Inserir(Guid.NewGuid(), Guid.NewGuid(), [new OrdemServicoServico(Guid.NewGuid(), Guid.NewGuid(), "Serviço", 100m)], []);
        return os;
    }
}

// ── Fakes ────────────────────────────────────────────────────────────────────

file class FakeOrdemServicoGateway : IOrdemServicoGateway
{
    private readonly OrdemServico? _os;
    public bool AtualizarFoiChamado { get; private set; }
    public bool SalvarFoiChamado    { get; private set; }
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

file class FakeAtualizarStatusPresenter : IAtualizarStatusOutputPort
{
    public bool NaoEncontradoChamado { get; private set; }
    public bool OkChamado            { get; private set; }
    public void NaoEncontrado() => NaoEncontradoChamado = true;
    public void Ok()            => OkChamado = true;
}