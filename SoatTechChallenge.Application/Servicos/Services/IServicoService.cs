using SoatTechChallenge.Application.Servicos.DTOs;
using SoatTechChallenge.Host.Common.DTOs;
using SoatTechChallenge.Host.Controllers.Servicos.DTOs;

namespace SoatTechChallenge.Application.Servicos.Services;

public interface IServicoService
{
    Task<ServicoResponse> Buscar(Guid id);
    Task<PagedResponse<ServicoResponse>> BuscarListaPaginada(PagedRequest request);
    Task<ServicoResponse> Inserir(InserirServicoRequest request);
    Task<ServicoResponse> Atualizar(Guid id, AtualizarServicoRequest request);
    Task Remover(Guid id);
}