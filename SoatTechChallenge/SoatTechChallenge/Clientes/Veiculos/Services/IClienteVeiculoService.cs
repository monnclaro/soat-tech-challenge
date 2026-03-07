using SoatTechChallenge.Application.DTOs;
using SoatTechChallenge.Clientes.Controllers.Veiculos.DTOs;

namespace SoatTechChallenge.Clientes.Veiculos.Services;

public interface IClienteVeiculoService
{
    Task<ClienteVeiculoResponse> Buscar(Guid id);
    Task<PagedResponse<ClienteVeiculoResponse>> BuscarListaPaginada(Guid idCliente, PagedRequest request);
    Task<ClienteVeiculoResponse> Inserir(Guid idCliente, InserirClienteVeiculoRequest request);
    Task<ClienteVeiculoResponse> Atualizar(Guid id, AtualizarClienteVeiculoRequest request);
    Task Remover(Guid id);
}