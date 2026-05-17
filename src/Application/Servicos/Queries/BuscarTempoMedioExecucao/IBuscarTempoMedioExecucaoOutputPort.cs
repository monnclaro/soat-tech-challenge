using Application.Servicos.DTOs;

namespace Application.Servicos.Queries.BuscarTempoMedioExecucao;

public interface IBuscarTempoMedioExecucaoOutputPort
{
    void Ok(IReadOnlyList<TempoMedioExecucaoOutput> resultado);
}
