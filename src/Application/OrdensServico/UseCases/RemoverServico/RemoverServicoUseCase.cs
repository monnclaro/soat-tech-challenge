using Application.Common.Markers;
using Domain.OrdensServico.Gateways;

namespace Application.OrdensServico.UseCases.RemoverServico;

public class RemoverServicoUseCase : IUseCase
{
    private readonly IOrdemServicoGateway _gateway;
    private readonly IRemoverServicoOrdemServicoOutputPort _outputPort;

    public RemoverServicoUseCase(IOrdemServicoGateway gateway, IRemoverServicoOrdemServicoOutputPort outputPort)
    {
        _gateway    = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(RemoverServicoOrdemServicoInput input, CancellationToken ct = default)
    {
        var ordemServico = await _gateway.BuscarComServicos(input.IdOrdemServico, ct);
        if (ordemServico is null) { _outputPort.NaoEncontrado(); return; }

        ordemServico.RemoverServico(input.IdServico);
        await _gateway.Atualizar(ordemServico, ct);
        _outputPort.Ok();
    }
}
