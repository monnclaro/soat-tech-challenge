using Application.Common.Interfaces;
using Application.OrdensServico.Queries;

namespace Application.OrdensServico.UseCases.BuscarOrdemServico;

public class BuscarOrdemServicoUseCase : IUseCase
{
    private readonly IOrdemServicoQueryGateway _gateway;
    private readonly IBuscarOrdemServicoOutputPort _outputPort;

    public BuscarOrdemServicoUseCase(IOrdemServicoQueryGateway gateway, IBuscarOrdemServicoOutputPort outputPort)
    {
        _gateway    = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(BuscarOrdemServicoInput input, CancellationToken ct = default)
    {
        var resultado = await _gateway.BuscarComDetalhes(input.Id, ct);

        if (resultado is null) { _outputPort.NaoEncontrado(); return; }

        _outputPort.Ok(resultado);
    }
}
