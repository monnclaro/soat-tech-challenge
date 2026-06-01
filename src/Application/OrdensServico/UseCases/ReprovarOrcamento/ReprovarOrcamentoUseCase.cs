using Application.Common.Interfaces;
using Domain.OrdensServico.Gateways;

namespace Application.OrdensServico.UseCases.ReprovarOrcamento;

public class ReprovarOrcamentoUseCase : IUseCase
{
    private readonly IOrdemServicoGateway _gateway;
    private readonly IReprovarOrcamentoOutputPort _outputPort;

    public ReprovarOrcamentoUseCase(IOrdemServicoGateway gateway, IReprovarOrcamentoOutputPort outputPort)
    {
        _gateway    = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(ReprovarOrcamentoInput input, CancellationToken ct = default)
    {
        var ordemServico = await _gateway.BuscarPorId(input.Id, ct);
        if (ordemServico is null) { _outputPort.NaoEncontrado(); return; }

        ordemServico.ReprovarOrcamento();
        await _gateway.Atualizar(ordemServico, ct);
        _outputPort.Ok();
    }
}
