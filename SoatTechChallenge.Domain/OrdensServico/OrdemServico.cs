using SharedKernel;
using SoatTechChallenge.Domain.Common.Exceptions;
using SoatTechChallenge.Domain.OrdensServico.Enums;
using SoatTechChallenge.Domain.OrdensServico.Events;
using SoatTechChallenge.Domain.OrdensServico.Produtos;
using SoatTechChallenge.Domain.OrdensServico.Servicos;
using SoatTechChallenge.Domain.OrdensServico.Servicos.Enums;

namespace SoatTechChallenge.Domain.OrdensServico;

public class OrdemServico : Entity
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
        Guid idVeiculo,
        List<OrdemServicoServico> servicos,
        List<OrdemServicoProduto> produtos)
    {
        Id = Guid.NewGuid();
        IdCliente = idCliente;
        IdVeiculo = idVeiculo;
        Status = StatusOrdemServico.Recebida;
        DataCriacao = DateTime.UtcNow;

        Servicos.AddRange(servicos);
        Produtos.AddRange(produtos);
        
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
    
    public void RemoverProduto(Guid idProduto)
    {
        if (Status != StatusOrdemServico.EmDiagnostico)
        {
            throw new DomainException("Só é possível remover produtos enquanto a ordem de serviço estiver em diagnóstico.");
        }

        var produto = Produtos.FirstOrDefault(s => s.Id == idProduto);
        if (produto is null)
        {
            throw new DomainException("O produto informado não se encontra vinculado a esta ordem de serviço.");
        }
        
        Produtos.Remove(produto);
        CalcularTotal();
    }

    public void RemoverServico(Guid idServico)
    {
        if (Status != StatusOrdemServico.EmDiagnostico)
        {
            throw new DomainException("Só é possível remover serviços enquanto a ordem de serviço estiver em diagnóstico.");
        }

        var servico = Servicos.FirstOrDefault(s => s.Id == idServico);
        if (servico is null)
        {
            throw new DomainException("O serviço informado não se encontra vinculado a esta ordem de serviço.");
        }

        Servicos.Remove(servico);
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

    public void FinalizarDiagnostico()
    {      
        if (!Servicos.Any())
        {
            throw new DomainException("Não é possível enviar o orçamento sem serviços vinculados.");
        }
        
        EnviarOrcamento();
    }

    private void EnviarOrcamento()
    {
        if (Status != StatusOrdemServico.EmDiagnostico)
        {
            throw new DomainException("O orçamento só pode ser enviado após diagnóstico.");
        }
        
        // Método mockado, pois não é um requisito do projeto no momento
        EnviarEmailOrcamento("cliente@email.com", "Assunto", $"Orçamento: {ValorTotal}");
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
    
    public void ReprovarOrcamento()
    {
        if (Status != StatusOrdemServico.AguardandoAprovacao)
        {
            throw new DomainException("O orçamento não está aguardando aprovação.");
        }
   
        Status = StatusOrdemServico.Finalizada;
    }

    public void IniciarExecucaoServico(Guid idServico)
    {
        if (Status != StatusOrdemServico.EmExecucao)
        {
            throw new DomainException("Não é possível iniciar a execução do serviço porque a ordem de serviço não está em execução.");
        }

        var servico = Servicos.FirstOrDefault(s => s.Id == idServico);
        if (servico is null)
        {
            throw new DomainException("O serviço informado não se encontra vinculado a esta ordem de serviço.");
        }

        servico.IniciarExecucao();
    }

    public void FinalizarExecucaoServico(Guid idServico)
    {
        if (Status != StatusOrdemServico.EmExecucao)
        {
            throw new DomainException("Não é possível finalizar a execução do serviço porque a ordem de serviço não está em execução.");
        }

        var servico = Servicos.FirstOrDefault(s => s.Id == idServico);
        if (servico is null)
        {
            throw new DomainException("O serviço informado não se encontra vinculado a esta ordem de serviço.");
        }

        servico.FinalizarExecucao();
        if (Servicos.All(s => s.Status == StatusOrdemServicoServico.ExecucaoFinalizada))
        {
            Finalizar();
        }
    }

    private void Finalizar()
    {
        DataFinalizacao = DateTime.UtcNow;
        Status = StatusOrdemServico.Finalizada;
        
        Raise(new OrdemServicoFinalizadaDomainEvent(Id, Produtos));
    }

    public void Entregar()
    {
        if (Status != StatusOrdemServico.Finalizada)
        {
            throw new DomainException("A entrega só pode ocorrer após a finalização de todos os serviços.");
        }
       
        Status = StatusOrdemServico.Entregue;
    }
    
    private void CalcularTotal()
    {
        ValorTotal = Servicos.Sum(s => s.Valor) + Produtos.Sum(p => p.Subtotal);
    }

    #region Helpers
    static void EnviarEmailOrcamento(string to, string subject, string body)
    {
        Console.WriteLine("=== MOCK EMAIL ===");
        Console.WriteLine($"Para: {to}");
        Console.WriteLine($"Assunto: {subject}");
        Console.WriteLine($"Mensagem: {body}");
        Console.WriteLine("==================");
    }
    #endregion
}