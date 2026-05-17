using Application.Common.Markers;
using Application.OrdensServico.Queries;
using SharedKernel;

namespace Application.OrdensServico.UseCases.BuscarListaPaginada;

public class BuscarListaPaginadaOrdemServicoUseCase : IUseCase
{
    private readonly IOrdemServicoQueryGateway _gateway;
    private readonly IBuscarListaPaginadaOrdemServicoOutputPort _outputPort;

    public BuscarListaPaginadaOrdemServicoUseCase(IOrdemServicoQueryGateway gateway, IBuscarListaPaginadaOrdemServicoOutputPort outputPort)
    {
        _gateway    = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(BuscarListaPaginadaOrdemServicoInput input, CancellationToken ct = default)
    {
        var (items, total) = await _gateway.BuscarPaginado(input.Paginacao, ct);
        _outputPort.Ok(new PagedResult<OrdemServicoOutput>(items.ToList(), total, input.Paginacao.Pagina, input.Paginacao.Tamanho));
    }
}
