using Application.Common.Markers;
using Application.Servicos.DTOs;

namespace Application.Servicos.Queries.BuscarTempoMedioExecucao;

public class BuscarTempoMedioExecucaoUseCase : IUseCase
{
    private readonly IServicoQueryGateway _gateway;
    private readonly IBuscarTempoMedioExecucaoOutputPort _outputPort;

    public BuscarTempoMedioExecucaoUseCase(IServicoQueryGateway gateway, IBuscarTempoMedioExecucaoOutputPort outputPort)
    {
        _gateway    = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(CancellationToken ct = default)
    {
        var resultado = await _gateway.BuscarTempoMedioExecucao(ct);

        var output = resultado
            .Select(r => new TempoMedioExecucaoOutput(
                r.Servico,
                r.TempoMedioMinutos,
                r.TempoMinimoMinutos,
                r.TempoMaximoMinutos))
            .ToList();

        _outputPort.Ok(output);
    }
}
