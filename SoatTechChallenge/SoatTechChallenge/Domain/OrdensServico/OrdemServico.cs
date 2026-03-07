using SoatTechChallenge.Domain.OrdensServico.Enums;
using SoatTechChallenge.Domain.OrdensServico.Produtos;
using SoatTechChallenge.Domain.OrdensServico.Servicos;
using SoatTechChallenge.Host.Controllers.OrdensServico.DTOs;
using SoatTechChallenge.Host.Middlewares.Exceptions;

namespace SoatTechChallenge.Domain.OrdensServico;

public class OrdemServico
{
    public Guid Id { get; private set; }
    public Guid IdCliente { get; private set; }
    public Guid IdVeiculo { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public DateTime? DataFinalizacao { get; private set; }
    public OrdemServicoStatus Status { get; private set; }
    public decimal ValorTotal { get; private set; }
    public List<OrdemServicoServico> Servicos { get; init; } = new();
    public List<OrdemServicoProduto> Produtos { get; init; } = new();

    public OrdemServico() { }

    public void Inserir(InserirOrdemServicoRequest request)
    {
        Id = Guid.NewGuid();
        IdCliente = request.IdCliente;
        IdVeiculo = request.IdVeiculo;
        Status = OrdemServicoStatus.Recebida;
        DataCriacao = DateTime.UtcNow;
    }

    public void Aprovar()
    {
        if (Status != OrdemServicoStatus.AguardandoAprovacao)
        {
            throw new DomainException("OS não está aguardando aprovação.");
        }
        
        Status = OrdemServicoStatus.EmExecucao;
    }

    public void Finalizar()
    {
        Status = OrdemServicoStatus.Finalizada;
        DataFinalizacao = DateTime.Now;
    }
    
    private void RecalcularTotal()
    {
        ValorTotal = Servicos.Sum(s => s.Preco) + Produtos.Sum(p => p.Subtotal);
    }
}