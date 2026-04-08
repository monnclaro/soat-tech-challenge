using SoatTechChallenge.Domain.Common.Exceptions;

namespace SoatTechChallenge.Domain.Servicos;

public class Servico
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public string Descricao { get; private set; }
    public decimal Valor { get; private set; }

    public Servico() { }

    public void Inserir(string nome, string descricao, decimal valor)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome do serviço é obrigatório.");

        if (valor < 0)
            throw new DomainException("O valor deve ser um número positivo.");

        Id = Guid.NewGuid();
        Nome = nome;
        Descricao = descricao;
        Valor = valor;
    }

    public void Atualizar(string nome, string descricao, decimal valor)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome do serviço é obrigatório.");

        if (valor < 0)
            throw new DomainException("O valor deve ser um número positivo.");

        Nome = nome;
        Descricao = descricao;
        Valor = valor;
    }
}