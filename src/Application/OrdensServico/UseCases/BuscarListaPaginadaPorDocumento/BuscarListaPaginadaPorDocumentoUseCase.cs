using Application.Common.Markers;
using Application.OrdensServico.Queries;
using Domain.OrdensServico.Gateways;
using SharedKernel;

namespace Application.OrdensServico.UseCases.BuscarListaPaginadaPorDocumento;

public class BuscarListaPaginadaPorDocumentoUseCase : IUseCase
{
    private readonly IOrdemServicoQueryGateway _gateway;
    private readonly IBuscarListaPaginadaPorDocumentoOutputPort _outputPort;

    public BuscarListaPaginadaPorDocumentoUseCase(IOrdemServicoQueryGateway gateway, IBuscarListaPaginadaPorDocumentoOutputPort outputPort)
    {
        _gateway    = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(BuscarListaPaginadaPorDocumentoInput input, CancellationToken ct = default)
    {
        var documentoLimpo = new string(input.Documento.Where(char.IsDigit).ToArray());
        var (items, total) = await _gateway.BuscarPaginadoPorDocumento(documentoLimpo, input.Paginacao, ct);
        _outputPort.Ok(new PagedResult<OrdemServicoPorDocumentoOutput>(items.ToList(), total, input.Paginacao.Pagina, input.Paginacao.Tamanho));
    }
}
