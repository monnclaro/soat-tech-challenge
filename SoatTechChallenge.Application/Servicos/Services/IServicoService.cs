using SoatTechChallenge.Application.Common.DTOs;
using SoatTechChallenge.Application.Servicos.DTOs;
using SoatTechChallenge.Application.Servicos.DTOs.Requests;
using SoatTechChallenge.Application.Servicos.DTOs.Responses;

namespace SoatTechChallenge.Application.Servicos.Services;

public interface IServicoService
{
    Task<ServicoResponse> Buscar(Guid id);
    Task<PagedResponse<ServicoResponse>> BuscarListaPaginada(PagedRequest request);
    Task<List<TempoMedioExecucaoServicosResponse>> BuscarTempoMedioExecucao();
    Task<ServicoResponse> Inserir(InserirServicoRequest request);
    Task<ServicoResponse> Atualizar(Guid id, AtualizarServicoRequest request);
    Task Remover(Guid id);
}