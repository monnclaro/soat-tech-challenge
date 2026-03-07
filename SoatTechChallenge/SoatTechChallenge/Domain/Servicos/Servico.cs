using SoatTechChallenge.Host.Controllers.Clientes.DTOs;
using SoatTechChallenge.Host.Controllers.Servicos.DTOs;
using SoatTechChallenge.Host.Middlewares.Exceptions;
using SoatTechChallenge.Middlewares.Exceptions;

namespace SoatTechChallenge.Domain.Servicos;

public class Servico
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public string Descricao { get; private set; }
    public decimal Preco { get; private set; }
    public int TempoEstimadoMinutos { get; private set; }

    public Servico() { }

    public void Inserir(InserirServicoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
            throw new DomainException("Nome do serviço é obrigatório.");

        if (request.Preco <= 0)
            throw new DomainException("Preço deve ser maior que zero.");

        if (request.TempoEstimadoMinutos <= 0)
            throw new DomainException("Tempo estimado deve ser maior que zero.");
        
        Id = Guid.NewGuid();
        Nome = request.Nome;
        Descricao = request.Descricao;
        Preco = request.Preco;
        TempoEstimadoMinutos = request.TempoEstimadoMinutos;
    }
    
    public void Atualizar(AtualizarServicoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
            throw new DomainException("Nome do serviço é obrigatório.");

        if (request.Preco <= 0)
            throw new DomainException("Preço deve ser maior que zero.");

        if (request.TempoEstimadoMinutos <= 0)
            throw new DomainException("Tempo estimado deve ser maior que zero.");
      
        Nome = request.Nome;
        Descricao = request.Descricao;
        Preco = request.Preco;
        TempoEstimadoMinutos = request.TempoEstimadoMinutos;
    }
}