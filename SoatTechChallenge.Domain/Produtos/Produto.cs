using SoatTechChallenge.Domain.Common.Exceptions;

namespace SoatTechChallenge.Domain.Produtos;

public class Produto
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public string Descricao { get; private set; }
    public decimal Valor { get; private set; }
    public decimal QuantidadeEmEstoque { get; private set; }

    public Produto() { }

    public void Inserir(string nome, string descricao, decimal valor, decimal quantidadeEmEstoque)
    {
        if (string.IsNullOrWhiteSpace(nome)) throw new DomainException("O nome do produto é obrigatório.");
        if (valor <= 0) throw new DomainException("O valor deve ser maior que zero.");
        if (quantidadeEmEstoque < 0) throw new DomainException("A quantidade em estoque não pode ser negativa.");
        
        Id = Guid.NewGuid();
        Nome = nome;
        Descricao = descricao;
        Valor = valor;
        QuantidadeEmEstoque = quantidadeEmEstoque;
    }
    
    public void Atualizar(string nome, string descricao, decimal valor )
    {
        if (string.IsNullOrWhiteSpace(nome)) throw new DomainException("O nome do produto é obrigatório.");
        if (valor <= 0) throw new DomainException("O valor deve ser maior que zero.");
     
        Nome = nome;
        Descricao = descricao;
        Valor = valor;
    }
    
    public void IncrementarQuantidadeEmEstoque(decimal quantidade)
    {
        QuantidadeEmEstoque += quantidade;
    }
    
    public void DecrementarQuantidadeEmEstoque(decimal quantidade)
    {
        QuantidadeEmEstoque -= quantidade;
    }
}