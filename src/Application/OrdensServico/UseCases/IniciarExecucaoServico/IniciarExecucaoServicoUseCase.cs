using Application.Common.Interfaces;
using Domain.OrdensServico.Gateways;

namespace Application.OrdensServico.UseCases.IniciarExecucaoServico;

public class IniciarExecucaoServicoUseCase : IUseCase
{
    private readonly IOrdemServicoGateway _gateway;
    private readonly IIniciarExecucaoServicoOutputPort _outputPort;

    public IniciarExecucaoServicoUseCase(IOrdemServicoGateway gateway, IIniciarExecucaoServicoOutputPort outputPort)
    {
        _gateway    = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(IniciarExecucaoServicoInput input, CancellationToken ct = default)
    {
        var ordemServico = await _gateway.BuscarComServicos(input.IdOrdemServico, ct);
        if (ordemServico is null) { _outputPort.NaoEncontrado(); return; }

        ordemServico.IniciarExecucaoServico(input.IdServico);
        await _gateway.Atualizar(ordemServico, ct);
        _outputPort.Ok();
    }
}
