using Application.Common.Markers;
using Domain.OrdensServico.Gateways;

namespace Application.OrdensServico.UseCases.IniciarDiagnostico;

public class IniciarDiagnosticoUseCase : IUseCase
{
    private readonly IOrdemServicoGateway _gateway;
    private readonly IIniciarDiagnosticoOutputPort _outputPort;

    public IniciarDiagnosticoUseCase(IOrdemServicoGateway gateway, IIniciarDiagnosticoOutputPort outputPort)
    {
        _gateway    = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(IniciarDiagnosticoInput input, CancellationToken ct = default)
    {
        var ordemServico = await _gateway.BuscarPorId(input.Id, ct);
        if (ordemServico is null) { _outputPort.NaoEncontrado(); return; }

        ordemServico.IniciarDiagnostico();
        await _gateway.Atualizar(ordemServico, ct);
        _outputPort.Ok();
    }
}
