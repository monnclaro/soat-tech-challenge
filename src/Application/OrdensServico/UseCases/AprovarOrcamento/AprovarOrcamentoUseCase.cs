using Application.Common.Markers;
using Domain.OrdensServico.Gateways;

namespace Application.OrdensServico.UseCases.AprovarOrcamento;

public class AprovarOrcamentoUseCase : IUseCase
{
    private readonly IOrdemServicoGateway _gateway;
    private readonly IAprovarOrcamentoOutputPort _outputPort;

    public AprovarOrcamentoUseCase(IOrdemServicoGateway gateway, IAprovarOrcamentoOutputPort outputPort)
    {
        _gateway    = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(AprovarOrcamentoInput input, CancellationToken ct = default)
    {
        var ordemServico = await _gateway.BuscarPorId(input.Id, ct);
        if (ordemServico is null) { _outputPort.NaoEncontrado(); return; }

        ordemServico.AprovarOrcamento();
        await _gateway.Atualizar(ordemServico, ct);
        _outputPort.Ok();
    }
}
