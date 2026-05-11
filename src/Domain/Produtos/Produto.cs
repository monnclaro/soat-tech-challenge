using Domain.Common.Exceptions;
using SharedKernel;

namespace Domain.Produtos;

public class Produto : Entity
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public string Descricao { get; private set; }
    public decimal Valor { get; private set; }
    public decimal QuantidadeEmEstoque { get; private set; }

    public Produto() { }

    public void Inserir(string nome, string descricao, decimal valor, decimal quantidadeEmEstoque)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome do produto é obrigatório.");

        if (valor < 0)
            throw new DomainException("O valor não pode ser negativo.");

        if (quantidadeEmEstoque < 0)
            throw new DomainException("A quantidade em estoque não pode ser negativa.");

        Id = Guid.NewGuid();
        Nome = nome;
        Descricao = descricao;
        Valor = valor;
        QuantidadeEmEstoque = quantidadeEmEstoque;
    }

    public void Atualizar(string nome, string descricao, decimal valor)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome do produto é obrigatório.");

        if (valor < 0)
            throw new DomainException("O valor não pode ser negativo.");

        Nome = nome;
        Descricao = descricao;
        Valor = valor;
    }

    public void IncrementarQuantidadeEmEstoque(decimal quantidade)
    {
        if (quantidade <= 0)
            throw new DomainException("A quantidade deve ser maior que zero.");

        QuantidadeEmEstoque += quantidade;
    }

    public void DecrementarQuantidadeEmEstoque(decimal quantidade)
    {
        if (quantidade < 0)
            throw new DomainException("A quantidade não pode ser negativa.");

        QuantidadeEmEstoque = Math.Max(0, QuantidadeEmEstoque - quantidade);
    }
}