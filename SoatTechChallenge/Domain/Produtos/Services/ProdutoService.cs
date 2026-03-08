using Microsoft.EntityFrameworkCore;
using SoatTechChallenge.Host.Common.DTOs;
using SoatTechChallenge.Host.Common.Services;
using SoatTechChallenge.Host.Controllers.Produtos.DTOs;
using SoatTechChallenge.Host.Middlewares.Exceptions;
using SoatTechChallenge.Infrastructure.Interfaces;

namespace SoatTechChallenge.Domain.Produtos.Services;

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
        var resultado = await _repository.Query().FirstOrDefaultAsync(l => l.Id == id);
        if (resultado is null)
        {
            throw new NotFoundException($"Produto de id: {id} não encontrado.");
        }
        
        return MapToResponse(resultado);
    }

    public async Task<PagedResponse<ProdutoResponse>> BuscarListaPaginada(PagedRequest request)
    {
        var query = _repository.Query().AsNoTracking()
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
        produto.Inserir(request);

        await _repository.AddAsync(produto);
        await _repository.SaveChangesAsync(); 
            
        return MapToResponse(produto);
    }

    public async Task<ProdutoResponse> Atualizar(Guid id, AtualizarProdutoRequest request)
    {
        var produto = await _repository.Query().FirstOrDefaultAsync(l => l.Id == id);
        if (produto is null)
        {
            throw new NotFoundException($"Produto de id: {id} não encontrado.");
        }

        produto.Atualizar(request);

        _repository.Update(produto);
        await _repository.SaveChangesAsync(); 
        
        return MapToResponse(produto);
    }
    
    public async Task<ProdutoResponse> Atualizar(Guid id, AtualizarQuantidadeEstoqueProdutoRequest request)
    {
        var produto = await _repository.Query().FirstOrDefaultAsync(l => l.Id == id);
        if (produto is null)
        {
            throw new NotFoundException($"Produto de id: {id} não encontrado.");
        }

        produto.AtualizarQuantidadeEmEstoque(request.QuantidadeEmEstoque);

        _repository.Update(produto);
        await _repository.SaveChangesAsync(); 
        
        return MapToResponse(produto);
    }

    public async Task Remover(Guid id)
    {
         var produto = await _repository.Query().FirstOrDefaultAsync(l => l.Id == id);
        if (produto is null)
        {
             throw new NotFoundException($"Produto de id: {id} não encontrado.");
        }
        
        _repository.Delete(produto);
        await _repository.SaveChangesAsync(); 
    }   
}