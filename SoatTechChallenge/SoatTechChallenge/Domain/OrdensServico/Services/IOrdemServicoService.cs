using SoatTechChallenge.Host.Common.DTOs;
using SoatTechChallenge.Host.Controllers.OrdensServico.DTOs.Requests;
using SoatTechChallenge.Host.Controllers.OrdensServico.DTOs.Responses;

namespace SoatTechChallenge.Domain.OrdensServico.Services;

public interface IOrdemServicoService
{
    Task<OrdemServicoResponse> Buscar(Guid id);
    Task<PagedResponse<OrdemServicoDetailedResponse>> BuscarListaPaginada(PagedRequest request);
    Task<TempoMedioExecucaoOrdensServicoResponse?> BuscarTempoMedioExecucao();
    Task<OrdemServicoResponse> Inserir(InserirOrdemServicoRequest request);
    Task IniciarDiagnostico(Guid id);
    Task EnviarOrcamento(Guid id);
    Task AprovarOrcamento(Guid id);
    Task IniciarExecucao(Guid id);
    Task Finalizar(Guid id);
    Task Entregar(Guid id);
    Task Remover(Guid id);
}