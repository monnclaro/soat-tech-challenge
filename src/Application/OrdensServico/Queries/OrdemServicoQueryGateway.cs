using Application.OrdensServico.UseCases;
using SharedKernel.DTOs;

namespace Application.OrdensServico.Queries;

public interface IOrdemServicoQueryGateway
{
    Task<OrdemServicoOutput?> BuscarComDetalhes(Guid id, CancellationToken ct = default);
    Task<OrdemServicoStatusOutput?> BuscarStatus(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<OrdemServicoOutput> Items, int Total)> BuscarPaginado(PagedRequest paginacao, CancellationToken ct = default);
    Task<(IReadOnlyList<OrdemServicoPorDocumentoOutput> Items, int Total)> BuscarPaginadoPorDocumento(string documento, PagedRequest paginacao, CancellationToken ct = default);
}