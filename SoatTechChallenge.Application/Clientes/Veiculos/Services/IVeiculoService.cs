using SoatTechChallenge.Application.Clientes.Veiculos.DTOs;
using SoatTechChallenge.Application.Clientes.Veiculos.DTOs.Requests;
using SoatTechChallenge.Application.Clientes.Veiculos.DTOs.Responses;
using SoatTechChallenge.Application.Common.DTOs;

namespace SoatTechChallenge.Application.Clientes.Veiculos.Services;

public interface IVeiculoService
{
    Task<VeiculoResponse> Buscar(Guid id);
    Task<PagedResponse<VeiculoResponse>> BuscarListaPaginada(Guid idCliente, PagedRequest request);
    Task<VeiculoResponse> Inserir(Guid idCliente, InserirVeiculoRequest request);
    Task<VeiculoResponse> Atualizar(Guid id, AtualizarVeiculoRequest request);
    Task Remover(Guid id);
}