using SoatTechChallenge.Host.Common.DTOs;
using SoatTechChallenge.Host.Controllers.Clientes.Veiculos.DTOs;

namespace SoatTechChallenge.Domain.Clientes.Veiculos.Services;

public interface IClienteVeiculoService
{
    Task<ClienteVeiculoResponse> Buscar(Guid id);
    Task<PagedResponse<ClienteVeiculoResponse>> BuscarListaPaginada(Guid idCliente, PagedRequest request);
    Task<ClienteVeiculoResponse> Inserir(Guid idCliente, InserirClienteVeiculoRequest request);
    Task<ClienteVeiculoResponse> Atualizar(Guid id, AtualizarClienteVeiculoRequest request);
    Task Remover(Guid id);
}