using Application.Servicos.DTOs;
using Domain.Servicos;
using SharedKernel.DTOs;

namespace Application.Servicos.Queries;

public interface IServicoQueryGateway
{
    Task<(IReadOnlyList<Servico> Items, int Total)> BuscarPaginado(string? filtro, PagedRequest paginacao, CancellationToken ct = default);
    Task<IReadOnlyList<TempoMedioExecucaoOutput>> BuscarTempoMedioExecucao(CancellationToken ct = default);
}