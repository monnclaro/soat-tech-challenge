using Application.OrdensServico.Queries;
using Application.OrdensServico.UseCases;
using Application.OrdensServico.UseCases.BuscarListaPaginadaPorDocumento;
using SharedKernel.DTOs;

namespace Tests.OrdensServico.Unit;

public class BuscarListaPaginadaPorDocumentoUseCaseTests
{
    [Fact]
    public async Task Execute_QuandoCallerDocumentoNulo_ConsultaGatewayNormalmente()
    {
        var gateway = new FakeGateway();
        var presenter = new FakePresenter();
        var useCase = new BuscarListaPaginadaPorDocumentoUseCase(gateway, presenter);

        await useCase.Execute(new BuscarListaPaginadaPorDocumentoInput("52998224725", new PagedRequest()), CancellationToken.None);

        Assert.True(gateway.Chamado);
        Assert.NotNull(presenter.Resultado);
    }

    [Fact]
    public async Task Execute_QuandoCallerDocumentoBateComDocumentoPesquisado_ConsultaGateway()
    {
        var gateway = new FakeGateway();
        var presenter = new FakePresenter();
        var useCase = new BuscarListaPaginadaPorDocumentoUseCase(gateway, presenter);

        await useCase.Execute(
            new BuscarListaPaginadaPorDocumentoInput("529.982.247-25", new PagedRequest(), CallerDocumento: "52998224725"),
            CancellationToken.None);

        Assert.True(gateway.Chamado);
    }

    [Fact]
    public async Task Execute_QuandoCallerDocumentoNaoBateComDocumentoPesquisado_NaoConsultaGatewayERetornaVazio()
    {
        var gateway = new FakeGateway();
        var presenter = new FakePresenter();
        var useCase = new BuscarListaPaginadaPorDocumentoUseCase(gateway, presenter);

        await useCase.Execute(
            new BuscarListaPaginadaPorDocumentoInput("52998224725", new PagedRequest(), CallerDocumento: "11144477735"),
            CancellationToken.None);

        Assert.False(gateway.Chamado);
        Assert.NotNull(presenter.Resultado);
        Assert.Empty(presenter.Resultado!.Items);
        Assert.Equal(0, presenter.Resultado.TotalCount);
    }

    private class FakeGateway : IOrdemServicoQueryGateway
    {
        public bool Chamado { get; private set; }

        public Task<OrdemServicoOutput?> BuscarComDetalhes(Guid id, CancellationToken ct = default) =>
            throw new NotImplementedException();

        public Task<OrdemServicoStatusOutput?> BuscarStatus(Guid id, CancellationToken ct = default) =>
            throw new NotImplementedException();

        public Task<(IReadOnlyList<OrdemServicoOutput> Items, int Total)> BuscarPaginado(PagedRequest paginacao, CancellationToken ct = default) =>
            throw new NotImplementedException();

        public Task<(IReadOnlyList<OrdemServicoPorDocumentoOutput> Items, int Total)> BuscarPaginadoPorDocumento(string documento, PagedRequest paginacao, CancellationToken ct = default)
        {
            Chamado = true;
            return Task.FromResult<(IReadOnlyList<OrdemServicoPorDocumentoOutput>, int)>(([], 0));
        }
    }

    private class FakePresenter : IBuscarListaPaginadaPorDocumentoOutputPort
    {
        public PagedResult<OrdemServicoPorDocumentoOutput>? Resultado { get; private set; }

        public void Ok(PagedResult<OrdemServicoPorDocumentoOutput> resultado) => Resultado = resultado;
    }
}
