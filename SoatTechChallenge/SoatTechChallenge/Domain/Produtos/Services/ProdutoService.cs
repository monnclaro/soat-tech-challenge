using Microsoft.EntityFrameworkCore;
using SoatTechChallenge.Application.DTOs;
using SoatTechChallenge.Host.Controllers.Produtos.DTOs;
using SoatTechChallenge.Infrastructure.Common;
using SoatTechChallenge.Infrastructure.Interfaces;
using SoatTechChallenge.Middlewares.Exceptions;

namespace SoatTechChallenge.Domain.Produtos.Services;

public class ProdutoService : IProdutoService, ITransientService
{
    private readonly IRepository<Produto> _repository;

    private static ProdutoResponse MapToResponse(Produto p) => new(p.Id, p.Nome, p.Descricao, p.Preco, p.QuantidadeEmEstoque);

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
        var resultado = await _repository.Query()
            .AsNoTracking()
            .OrderBy(l => l.Nome)
            .Skip((request.Pagina - 1) * request.Tamanho)
            .Take(request.Tamanho)
            .Select(p => new ProdutoResponse(p.Id, p.Nome, p.Descricao, p.Preco, p.QuantidadeEmEstoque))
            .ToListAsync();

        return new PagedResponse<ProdutoResponse>(resultado);
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