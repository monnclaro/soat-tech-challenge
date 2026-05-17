using Application.Common.Markers;
using Domain.OrdensServico.Gateways;

namespace Application.OrdensServico.UseCases.FinalizarDiagnostico;

public class FinalizarDiagnosticoUseCase : IUseCase
{
    private readonly IOrdemServicoGateway _gateway;
    private readonly IFinalizarDiagnosticoOutputPort _outputPort;

    public FinalizarDiagnosticoUseCase(IOrdemServicoGateway gateway, IFinalizarDiagnosticoOutputPort outputPort)
    {
        _gateway    = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(FinalizarDiagnosticoInput input, CancellationToken ct = default)
    {
        var ordemServico = await _gateway.BuscarComServicosProdutos(input.Id, ct);
        if (ordemServico is null) { _outputPort.NaoEncontrado(); return; }

        ordemServico.FinalizarDiagnostico();
        await _gateway.Atualizar(ordemServico, ct);
        _outputPort.Ok();
    }
}
