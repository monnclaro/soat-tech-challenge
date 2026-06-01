using Application.Common.Interfaces;
using Application.Servicos.DTOs;
using SharedKernel.DTOs;

namespace Application.Servicos.Queries.BuscarListaPaginada;

public class BuscarListaPaginadaUseCase : IUseCase
{
    private readonly IServicoQueryGateway _gateway;
    private readonly IBuscarListaPaginadaOutputPort _outputPort;

    public BuscarListaPaginadaUseCase(IServicoQueryGateway gateway, IBuscarListaPaginadaOutputPort outputPort)
    {
        _gateway    = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(BuscarListaPaginadaInput input, CancellationToken ct = default)
    {
        var (items, total) = await _gateway.BuscarPaginado(null, input.Paginacao, ct);

        var output = new PagedResult<ServicoOutput>(
            items.Select(s => new ServicoOutput(s.Id, s.Nome, s.Descricao, s.Valor)).ToList(),
            total,
            input.Paginacao.Pagina,
            input.Paginacao.Tamanho);

        _outputPort.Ok(output);
    }
}
