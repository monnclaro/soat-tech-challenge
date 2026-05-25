using Application.Common.Interfaces;
using Application.OrdensServico.UseCases.AtualizarStatus.DTOs;
using Domain.OrdensServico.Gateways;

namespace Application.OrdensServico.UseCases.AtualizarStatus;

public class AtualizarStatusUseCase : IUseCase
{
    private readonly IOrdemServicoGateway _gateway;
    private readonly IAtualizarStatusOutputPort _outputPort;

    public AtualizarStatusUseCase(IOrdemServicoGateway gateway, IAtualizarStatusOutputPort outputPort)
    {
        _gateway    = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(AtualizarStatusOrdemServicoInput input, CancellationToken ct = default)
    {
        var os = await _gateway.BuscarPorId(input.Id, ct);
        if (os is null) { _outputPort.NaoEncontrado(); return; }

        os.AtualizarStatus(input.Status);

        await _gateway.Atualizar(os, ct);
        _outputPort.Ok();
    }
}