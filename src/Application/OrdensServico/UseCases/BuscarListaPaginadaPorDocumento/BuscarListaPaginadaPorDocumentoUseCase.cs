using Application.Common.Interfaces;
using Application.OrdensServico.Queries;
using SharedKernel.DTOs;

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

        if (input.CallerDocumento is { } callerDocumento)
        {
            var callerDocumentoLimpo = new string(callerDocumento.Where(char.IsDigit).ToArray());
            if (callerDocumentoLimpo != documentoLimpo)
            {
                // Resultado vazio (não erro) de propósito: não confirma pra um
                // Cliente se existe OS pra outro documento.
                _outputPort.Ok(new PagedResult<OrdemServicoPorDocumentoOutput>([], 0, input.Paginacao.Pagina, input.Paginacao.Tamanho));
                return;
            }
        }

        var (items, total) = await _gateway.BuscarPaginadoPorDocumento(documentoLimpo, input.Paginacao, ct);
        _outputPort.Ok(new PagedResult<OrdemServicoPorDocumentoOutput>(items.ToList(), total, input.Paginacao.Pagina, input.Paginacao.Tamanho));
    }
}
