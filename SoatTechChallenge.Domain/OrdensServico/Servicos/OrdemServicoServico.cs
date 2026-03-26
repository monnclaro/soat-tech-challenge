using SoatTechChallenge.Domain.Common.Exceptions;
using SoatTechChallenge.Domain.OrdensServico.Servicos.Enums;

namespace SoatTechChallenge.Domain.OrdensServico.Servicos;

public class OrdemServicoServico
{
    public Guid Id { get; private set; }
    public Guid IdOrdemServico { get; private set; }
    public Guid IdServico { get; private set; }
    public string NomeServico { get; private set; }
    public decimal Valor { get; private set; }
    public StatusOrdemServicoServico Status { get; private set; }
    public DateTime? DataInicioExecucao { get; private set; }
    public DateTime? DataFinalizacaoExecucao { get; private set; }
    
    public OrdemServicoServico() { }
    
    public OrdemServicoServico(Guid idOrdemServico, Guid idServico, string nomeServico, decimal valor)
    {
        Id = Guid.NewGuid();
        IdOrdemServico = idOrdemServico;
        IdServico = idServico;
        NomeServico = nomeServico;
        Valor = valor;
        Status = StatusOrdemServicoServico.AguardandoExecucao;
    }

    public void IniciarExecucao()
    {
        if (Status is StatusOrdemServicoServico.ExecucaoFinalizada)
        {
            throw new DomainException("O serviço já se encontra finalizado.");
        }

        Status = StatusOrdemServicoServico.EmExecucao;
        DataInicioExecucao = DateTime.UtcNow;
    }
    
    public void FinalizarExecucao()
    {
        if (Status is StatusOrdemServicoServico.ExecucaoFinalizada)
        {
            throw new DomainException("O serviço já se encontra finalizado.");
        }

        Status = StatusOrdemServicoServico.ExecucaoFinalizada;
        DataFinalizacaoExecucao = DateTime.UtcNow;
    }
}