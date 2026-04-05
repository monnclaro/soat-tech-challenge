using SoatTechChallenge.Application.Clientes.DTOs;
using SoatTechChallenge.Application.Clientes.DTOs.Requests;
using SoatTechChallenge.Application.Clientes.DTOs.Responses;
using SoatTechChallenge.Application.Common.DTOs;

namespace SoatTechChallenge.Application.Clientes.Services;

public interface IClienteService
{
    Task<ClienteResponse> Buscar(Guid id);
    Task<PagedResponse<ClienteResponse>> BuscarListaPaginada(PagedRequest request);
    Task<ClienteResponse> Inserir(InserirClienteRequest request);
    Task<ClienteResponse> Atualizar(Guid id, AtualizarClienteRequest request);
    Task Remover(Guid id);
}