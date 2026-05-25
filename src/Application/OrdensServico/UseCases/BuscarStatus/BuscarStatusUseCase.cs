using Application.Common.Interfaces;
using Application.OrdensServico.Queries;

namespace Application.OrdensServico.UseCases.BuscarStatus;

public class BuscarStatusUseCase : IUseCase
{
    private readonly IOrdemServicoQueryGateway _gateway;
    private readonly IBuscarStatusOutputPort _outputPort;

    public BuscarStatusUseCase(IOrdemServicoQueryGateway gateway, IBuscarStatusOutputPort outputPort)
    {
        _gateway    = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(BuscarStatusInput input, CancellationToken ct = default)
    {
        var resultado = await _gateway.BuscarStatus(input.Id, ct);

        if (resultado is null) { _outputPort.NaoEncontrado(); return; }

        _outputPort.Ok(resultado);
    }
}
