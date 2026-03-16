using SoatTechChallenge.Application.Common.DTOs;
using SoatTechChallenge.Application.OrdensServico.DTOs.Requests;
using SoatTechChallenge.Application.OrdensServico.DTOs.Responses;
using SoatTechChallenge.Host.Controllers.OrdensServico.DTOs.Responses;

namespace SoatTechChallenge.Application.OrdensServico.Services;

public interface IOrdemServicoService
{
    #region Reads

    Task<OrdemServicoResponse?> Buscar(Guid id);
    Task<PagedResponse<OrdemServicoResponse>> BuscarListaPaginada(PagedRequest request);
    Task<PagedResponse<OrdemServicoPorDocumentoResponse>> BuscarListaPaginadaPorDocumento(string documento, PagedRequest request);

    #endregion
   
    #region Writes
    
    Task Inserir(InserirOrdemServicoRequest request);
    Task InserirProdutos(Guid id, InserirProdutosOrdemServicoRequest request);
    Task InserirServicos(Guid id, InserirServicosOrdemServicoRequest request);
    Task IniciarDiagnostico(Guid id);
    Task FinalizarDiagnostico(Guid id);
    Task AprovarOrcamento(Guid id);
    Task IniciarExecucaoServico(Guid id, Guid idServico);
    Task FinalizarExecucaoServico(Guid id, Guid idServico);
    Task Entregar(Guid id);
    Task Remover(Guid id);
    Task RemoverProduto(Guid id, Guid idProduto);
    Task RemoverServico(Guid id, Guid idServico);

    #endregion
}