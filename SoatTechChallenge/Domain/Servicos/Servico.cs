using SoatTechChallenge.Host.Controllers.Servicos.DTOs;
using SoatTechChallenge.Host.Middlewares.Exceptions;

namespace SoatTechChallenge.Domain.Servicos;

public class Servico
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public string Descricao { get; private set; }
    public decimal Valor { get; private set; }

    public Servico() { }

    public void Inserir(InserirServicoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
            throw new DomainException("Nome do serviço é obrigatório.");

        if (request.Valor <= 0)
            throw new DomainException("Preço deve ser maior que zero.");
    
        Id = Guid.NewGuid();
        Nome = request.Nome;
        Descricao = request.Descricao;
        Valor = request.Valor;
    }
    
    public void Atualizar(AtualizarServicoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
            throw new DomainException("Nome do serviço é obrigatório.");

        if (request.Valor <= 0)
            throw new DomainException("Preço deve ser maior que zero.");
   
        Nome = request.Nome;
        Descricao = request.Descricao;
        Valor = request.Valor;
    }
}