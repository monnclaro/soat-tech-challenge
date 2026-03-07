namespace SoatTechChallenge.Domain.OrdensServico.Servicos;

public class OrdemServicoServico
{
    public Guid Id { get; private set; }
    public Guid IdOrdemServico { get; private set; }
    public Guid IdServico { get; private set; }
    public decimal Preco { get; private set; }
    
    public OrdemServicoServico() { }
}