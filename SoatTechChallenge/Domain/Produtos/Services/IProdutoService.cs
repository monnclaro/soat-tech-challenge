using SoatTechChallenge.Host.Common.DTOs;
using SoatTechChallenge.Host.Controllers.Clientes.DTOs;
using SoatTechChallenge.Host.Controllers.Produtos.DTOs;

namespace SoatTechChallenge.Domain.Produtos.Services;

public interface IProdutoService
{
    Task<ProdutoResponse> Buscar(Guid id);
    Task<PagedResponse<ProdutoResponse>> BuscarListaPaginada(PagedRequest request);
    Task<ProdutoResponse> Inserir(InserirProdutoRequest request);
    Task<ProdutoResponse> Atualizar(Guid id, AtualizarProdutoRequest request);
    Task<ProdutoResponse> Atualizar(Guid id, AtualizarQuantidadeEstoqueProdutoRequest request);
    Task Remover(Guid id);
}