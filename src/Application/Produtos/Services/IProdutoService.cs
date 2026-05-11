using Application.Common.DTOs;
using Application.Produtos.DTOs.Commands;
using Application.Produtos.DTOs.Requests;
using Application.Produtos.DTOs.Responses;

namespace Application.Produtos.Services;

public interface IProdutoService
{
    Task<ProdutoResponse> Buscar(Guid id);
    Task<PagedResponse<ProdutoResponse>> BuscarListaPaginada(PagedRequest request);
    Task<ProdutoResponse> Inserir(InserirProdutoRequest request);
    Task<ProdutoResponse> Atualizar(Guid id, AtualizarProdutoRequest request);
    Task<ProdutoResponse> IncrementarEstoque(Guid id, AtualizarQuantidadeEstoqueProdutoRequest request);
    Task DecrementarEstoque(DecrementarQuantidadeEstoqueProdutosCommand command);
    Task Remover(Guid id);
}