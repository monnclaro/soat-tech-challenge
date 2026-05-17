using Application.Common.Markers;
using Domain.OrdensServico.Gateways;

namespace Application.OrdensServico.UseCases.FinalizarExecucaoServico;

public class FinalizarExecucaoServicoUseCase : IUseCase
{
    private readonly IOrdemServicoGateway _gateway;
    private readonly IFinalizarExecucaoServicoOutputPort _outputPort;

    public FinalizarExecucaoServicoUseCase(IOrdemServicoGateway gateway, IFinalizarExecucaoServicoOutputPort outputPort)
    {
        _gateway    = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(FinalizarExecucaoServicoInput input, CancellationToken ct = default)
    {
        var ordemServico = await _gateway.BuscarComServicosProdutos(input.IdOrdemServico, ct);
        if (ordemServico is null) { _outputPort.NaoEncontrado(); return; }

        ordemServico.FinalizarExecucaoServico(input.IdServico);
        await _gateway.Atualizar(ordemServico, ct);
        _outputPort.Ok();
    }
}