using SoatTechChallenge.Host.Common.DTOs;
using SoatTechChallenge.Host.Controllers.OrdensServico.DTOs.Requests;
using SoatTechChallenge.Host.Controllers.OrdensServico.DTOs.Responses;

namespace SoatTechChallenge.Domain.OrdensServico.Services;

public interface IOrdemServicoService
{
    #region Reads

    Task<OrdemServicoResponse?> Buscar(Guid id);
    Task<PagedResponse<OrdemServicoResponse>> BuscarListaPaginada(PagedRequest request);
    Task<PagedResponse<OrdemServicoPorDocumentoResponse>> BuscarListaPaginadaPorDocumento(string documento, PagedRequest request);
    Task<TempoMedioExecucaoOrdensServicoResponse?> BuscarTempoMedioExecucao();

    #endregion
   
    #region Writes
    
    Task<OrdemServicoOrcamentoResponse> Inserir(InserirOrdemServicoRequest request);
    Task<OrdemServicoOrcamentoResponse> Atualizar(Guid id, AtualizarOrdemServicoRequest request);
    Task IniciarDiagnostico(Guid id);
    Task<OrdemServicoOrcamentoResponse> EnviarOrcamento(Guid id);
    Task AprovarOrcamento(Guid id);
    Task Finalizar(Guid id);
    Task Entregar(Guid id);
    Task Remover(Guid id);
    
    #endregion
}