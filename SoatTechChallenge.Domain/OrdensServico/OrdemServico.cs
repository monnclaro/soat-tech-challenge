using SoatTechChallenge.Domain.Common.Exceptions;
using SoatTechChallenge.Domain.OrdensServico.Enums;
using SoatTechChallenge.Domain.OrdensServico.Produtos;
using SoatTechChallenge.Domain.OrdensServico.Servicos;
using SoatTechChallenge.Domain.Produtos;
using SoatTechChallenge.Domain.Servicos;


namespace SoatTechChallenge.Domain.OrdensServico;

public class OrdemServico
{
    public Guid Id { get; private set; }
    public Guid IdCliente { get; private set; }
    public Guid IdVeiculo { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public DateTime? DataInicioExecucao { get; private set; }
    public DateTime? DataFinalizacao { get; private set; }
    public StatusOrdemServico Status { get; private set; }
    public decimal ValorTotal { get; private set; }
    public List<OrdemServicoServico> Servicos { get; init; } = new();
    public List<OrdemServicoProduto> Produtos { get; init; } = new();
    
    #region NotMapped
    
    public List<Guid> IdsProdutos => Produtos.Select(p => p.IdProduto).ToList();
    public List<Guid> IdsServicos => Servicos.Select(p => p.IdServico).ToList();
    
    #endregion

    public OrdemServico() { }

    public void Inserir(
        Guid idCliente,
        Guid idVeiculo)
    {
        Id = Guid.NewGuid();
        IdCliente = idCliente;
        IdVeiculo = idVeiculo;
        Status = StatusOrdemServico.Recebida;
        DataCriacao = DateTime.UtcNow;

        CalcularTotal();
    }

    public void InserirProdutos(List<OrdemServicoProduto> produtos)
    {
        if (Status is not StatusOrdemServico.EmDiagnostico)
        {
            throw new DomainException("Só é possível adicionar produtos enquanto a ordem de serviço estiver em diagnóstico.");
        }
        
        Produtos.AddRange(produtos);
        CalcularTotal();
    }

    public void InserirServicos(List<OrdemServicoServico> servicos)
    {
        if (Status is not StatusOrdemServico.EmDiagnostico)
        {
            throw new DomainException("Só é possível adicionar serviços enquanto a ordem de serviço estiver em diagnóstico.");
        }

        Servicos.AddRange(servicos);
        CalcularTotal();
    }

    public void IniciarDiagnostico()
    {
        if (Status != StatusOrdemServico.Recebida)
        {
            throw new DomainException("O diagnóstico só ser iniciado após recebimento.");
        }

        Status = StatusOrdemServico.EmDiagnostico;
    }

    public void EnviarOrcamento()
    {
        if (Status != StatusOrdemServico.EmDiagnostico)
        {
            throw new DomainException("O orçamento só pode ser enviado após diagnóstico.");
        }
        
        if (!Servicos.Any() && !Produtos.Any())
        {
            throw new DomainException("Não é possível enviar o orçamento sem serviços ou produtos.");
        }
        
        Status = StatusOrdemServico.AguardandoAprovacao;
    }

    public void AprovarOrcamento()
    {
        if (Status != StatusOrdemServico.AguardandoAprovacao)
        {
            throw new DomainException("O orçamento não está aguardando aprovação.");
        }
        
        DataInicioExecucao = DateTime.UtcNow;
        Status = StatusOrdemServico.EmExecucao;
    }

    public void FinalizarServico()
    {
        if (Status != StatusOrdemServico.EmExecucao)
        {
            throw new DomainException("O serviço não está em execução.");
        }
       
        DataFinalizacao = DateTime.UtcNow;
        Status = StatusOrdemServico.Finalizada;
    }

    public void Entregar()
    {
        if (Status != StatusOrdemServico.Finalizada)
        {
            throw new DomainException("A entrega só pode ocorrer após finalização.");
        }
       
        Status = StatusOrdemServico.Entregue;
    }
    
    private void CalcularTotal()
    {
        ValorTotal = Servicos.Sum(s => s.Valor) + Produtos.Sum(p => p.Subtotal);
    }
}