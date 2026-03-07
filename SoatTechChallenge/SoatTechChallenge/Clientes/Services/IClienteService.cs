using SoatTechChallenge.Application.DTOs;
using SoatTechChallenge.Clientes.Controllers.DTOs;

namespace SoatTechChallenge.Clientes.Services;

public interface IClienteService
{
    Task<ClienteResponse> Buscar(Guid id);
    Task<PagedResponse<ClienteResponse>> BuscarListaPaginada(PagedRequest request);
    Task<ClienteResponse> Inserir(InserirClienteRequest request);
    Task<ClienteResponse> Atualizar(Guid id, AtualizarClienteRequest request);
    Task Remover(Guid id);
}