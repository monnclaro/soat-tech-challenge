namespace SoatTechChallenge.Domain.OrdensServico.Servicos;

public class OrdemServicoServico
{
    public Guid Id { get; private set; }
    public Guid IdOrdemServico { get; private set; }
    public Guid IdServico { get; private set; }
    public string NomeServico { get; private set; }
    public decimal Valor { get; private set; }
    
    public OrdemServicoServico() { }
    
    public OrdemServicoServico(Guid idOrdemServico, Guid idServico, string nomeServico, decimal valor)
    {
        Id = Guid.NewGuid();
        IdOrdemServico = idOrdemServico;
        IdServico = idServico;
        NomeServico = nomeServico;
        Valor = valor;
    }
}