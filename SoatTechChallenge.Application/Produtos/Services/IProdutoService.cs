using SoatTechChallenge.Application.Common.DTOs;
using SoatTechChallenge.Application.Produtos.DTOs;
using SoatTechChallenge.Host.Controllers.Produtos.DTOs;

namespace SoatTechChallenge.Application.Produtos.Services;

public interface IProdutoService
{
    Task<ProdutoResponse> Buscar(Guid id);
    Task<PagedResponse<ProdutoResponse>> BuscarListaPaginada(PagedRequest request);
    Task<ProdutoResponse> Inserir(InserirProdutoRequest request);
    Task<ProdutoResponse> Atualizar(Guid id, AtualizarProdutoRequest request);
    Task<ProdutoResponse> IncrementarEstoque(Guid id, AtualizarQuantidadeEstoqueProdutoRequest request);
    Task Remover(Guid id);
}