using Application.Clientes.DTOs.Requests;
using Application.Clientes.DTOs.Responses;
using Application.Common.DTOs;

namespace Application.Clientes.Services;

public interface IClienteService
{
    Task<ClienteResponse> Buscar(Guid id);
    Task<PagedResponse<ClienteResponse>> BuscarListaPaginada(PagedRequest request);
    Task<ClienteResponse> Inserir(InserirClienteRequest request);
    Task<ClienteResponse> Atualizar(Guid id, AtualizarClienteRequest request);
    Task Remover(Guid id);
}