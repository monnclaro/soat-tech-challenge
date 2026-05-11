using Application.Clientes.Veiculos.DTOs.Requests;
using Application.Clientes.Veiculos.DTOs.Responses;
using Application.Common.DTOs;

namespace Application.Clientes.Veiculos.Services;

public interface IVeiculoService
{
    Task<VeiculoResponse> Buscar(Guid id);
    Task<PagedResponse<VeiculoResponse>> BuscarListaPaginada(Guid idCliente, PagedRequest request);
    Task<VeiculoResponse> Inserir(Guid idCliente, InserirVeiculoRequest request);
    Task<VeiculoResponse> Atualizar(Guid id, AtualizarVeiculoRequest request);
    Task Remover(Guid id);
}