using SoatTechChallenge.Host.Controllers.Produtos.DTOs;
using SoatTechChallenge.Host.Middlewares.Exceptions;

namespace SoatTechChallenge.Domain.Produtos;

public class Produto
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public string Descricao { get; private set; }
    public decimal Valor { get; private set; }
    public decimal QuantidadeEmEstoque { get; private set; }

    public Produto() { }

    public void Inserir(InserirProdutoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
            throw new DomainException("Nome do produto é obrigatório.");

        if (request.Valor <= 0)
            throw new DomainException("Preço deve ser maior que zero.");

        if (request.QuantidadeEmEstoque < 0)
            throw new DomainException("Quantidade em estoque não pode ser negativa.");
        
        Id = Guid.NewGuid();
        Nome = request.Nome;
        Descricao = request.Descricao;
        Valor = request.Valor;
        QuantidadeEmEstoque = request.QuantidadeEmEstoque;
    }
    
    public void Atualizar(AtualizarProdutoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
            throw new DomainException("Nome do produto é obrigatório.");

        if (request.Valor <= 0)
            throw new DomainException("Preço deve ser maior que zero.");

        if (request.QuantidadeEmEstoque < 0)
            throw new DomainException("Quantidade em estoque não pode ser negativa.");
     
        Nome = request.Nome;
        Descricao = request.Descricao;
        Valor =request.Valor;
        QuantidadeEmEstoque = request.QuantidadeEmEstoque;
    }
    
    public void AtualizarQuantidadeEmEstoque(decimal quantidade)
    {
        QuantidadeEmEstoque = quantidade;
    }
    
    public void DecrementarQuantidadeEmEstoque(decimal quantidade)
    {
        QuantidadeEmEstoque -= quantidade;
    }
}