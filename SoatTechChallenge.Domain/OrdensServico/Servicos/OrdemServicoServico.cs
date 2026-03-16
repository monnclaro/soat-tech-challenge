using System.ComponentModel.DataAnnotations.Schema;
using SoatTechChallenge.Domain.Common.Exceptions;

namespace SoatTechChallenge.Domain.OrdensServico.Servicos;

public class OrdemServicoServico
{
    public Guid Id { get; private set; }
    public Guid IdOrdemServico { get; private set; }
    public Guid IdServico { get; private set; }
    public string NomeServico { get; private set; }
    public decimal Valor { get; private set; }
    public DateTime? DataInicioExecucao { get; private set; }
    public DateTime? DataFinalizacaoExecucao { get; private set; }
    
    #region NotMapped
    
    public bool Finalizado => DataFinalizacaoExecucao.HasValue;
    
    #endregion
    
    public OrdemServicoServico() { }
    
    public OrdemServicoServico(Guid idOrdemServico, Guid idServico, string nomeServico, decimal valor)
    {
        Id = Guid.NewGuid();
        IdOrdemServico = idOrdemServico;
        IdServico = idServico;
        NomeServico = nomeServico;
        Valor = valor;
    }

    public void IniciarExecucao()
    {
        if (Finalizado)
        {
            throw new DomainException("O serviço já se encontra finalizado.");
        }
        
        DataInicioExecucao = DateTime.UtcNow;
    }
    
    public void FinalizarExecucao()
    {
        if (Finalizado)
        {
            throw new DomainException("O serviço já se encontra finalizado.");
        }
        
        DataFinalizacaoExecucao = DateTime.UtcNow;
    }
}