using Application.Common.DTOs;
using Application.Servicos.DTOs.Requests;
using Application.Servicos.DTOs.Responses;

namespace Application.Servicos.Services;

public interface IServicoService
{
    Task<ServicoResponse> Buscar(Guid id);
    Task<PagedResponse<ServicoResponse>> BuscarListaPaginada(PagedRequest request);
    Task<List<TempoMedioExecucaoServicosResponse>> BuscarTempoMedioExecucao();
    Task<ServicoResponse> Inserir(InserirServicoRequest request);
    Task<ServicoResponse> Atualizar(Guid id, AtualizarServicoRequest request);
    Task Remover(Guid id);
}