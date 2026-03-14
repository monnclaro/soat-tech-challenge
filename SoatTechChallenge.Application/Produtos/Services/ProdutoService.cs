using Microsoft.EntityFrameworkCore;
using SoatTechChallenge.Application.Common.Interfaces;
using SoatTechChallenge.Domain.Common.Exceptions;
using SoatTechChallenge.Domain.Common.Interfaces;
using SoatTechChallenge.Domain.Produtos;
using SoatTechChallenge.Host.Common.DTOs;
using SoatTechChallenge.Host.Controllers.Produtos.DTOs;

namespace SoatTechChallenge.Application.Produtos.Services;

public class ProdutoService : IProdutoService, IScopedService
{
    private readonly IRepository<Produto> _repository;

    private static ProdutoResponse MapToResponse(Produto p) => new(p.Id, p.Nome, p.Descricao, p.Valor, p.QuantidadeEmEstoque);

    public ProdutoService(IRepository<Produto> repository)
    {
        _repository = repository;
    }

    public async Task<ProdutoResponse> Buscar(Guid id)
    {
        var resultado = await _repository.GetQueryable().FirstOrDefaultAsync(l => l.Id == id);
        if (resultado is null)
        {
            throw new NotFoundException($"Produto de id: {id} não encontrado.");
        }

        return MapToResponse(resultado);
    }

    public async Task<PagedResponse<ProdutoResponse>> BuscarListaPaginada(PagedRequest request)
    {
        var query = _repository.GetQueryable().AsNoTracking()
            .OrderBy(l => l.Nome)
            .Select(p => new ProdutoResponse(p.Id, p.Nome, p.Descricao, p.Valor, p.QuantidadeEmEstoque));

        var total = await query.CountAsync();
        var resultado = await query
            .Skip((request.Pagina - 1) * request.Tamanho)
            .Take(request.Tamanho)
            .ToListAsync();

        return new PagedResponse<ProdutoResponse>(resultado, total, request.Pagina, request.Tamanho);
    }

    public async Task<ProdutoResponse> Inserir(InserirProdutoRequest request)
    {
        var produto = new Produto();
        produto.Inserir(request.Nome, request.Descricao, request.Valor, request.QuantidadeEmEstoque);

        await _repository.InsertAsync(produto);

        return MapToResponse(produto);
    }

    public async Task<ProdutoResponse> Atualizar(Guid id, AtualizarProdutoRequest request)
    {
        var produto = await _repository.GetQueryable().FirstOrDefaultAsync(l => l.Id == id);
        if (produto is null)
        {
            throw new NotFoundException($"Produto de id: {id} não encontrado.");
        }

        produto.Atualizar(request.Nome, request.Descricao, request.Valor, request.QuantidadeEmEstoque);

        await _repository.UpdateAsync(produto);

        return MapToResponse(produto);
    }

    public async Task<ProdutoResponse> Atualizar(Guid id, AtualizarQuantidadeEstoqueProdutoRequest request)
    {
        var produto = await _repository.GetQueryable().FirstOrDefaultAsync(l => l.Id == id);
        if (produto is null)
        {
            throw new NotFoundException($"Produto de id: {id} não encontrado.");
        }

        produto.AtualizarQuantidadeEmEstoque(request.QuantidadeEmEstoque);

        await _repository.UpdateAsync(produto);
        return MapToResponse(produto);
    }

    public async Task Remover(Guid id)
    {
        var produto = await _repository.GetQueryable().FirstOrDefaultAsync(l => l.Id == id);
        if (produto is null)
        {
            throw new NotFoundException($"Produto de id: {id} não encontrado.");
        }

        await _repository.DeleteAsync(produto.Id);
    }
}