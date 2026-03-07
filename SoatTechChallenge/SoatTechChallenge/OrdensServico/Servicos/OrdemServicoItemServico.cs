/*namespace SoatTechChallenge.OrdensServico.Servicos;

public class OrdemServicoItemServico
{
    public Guid Id { get; private set; }
    public Guid OrdemServicoId { get; private set; }
    public Guid ServicoId { get; private set; }
    public string NomeServico { get; private set; }
    public decimal PrecoUnitario { get; private set; }
    public int Quantidade { get; private set; }
    public decimal Subtotal => PrecoUnitario * Quantidade;

    protected ItemServico() { }

    public ItemServico(Guid osId, Guid servicoId, string nome,
        decimal preco, int quantidade)
    {
        Id = Guid.NewGuid();
        OrdemServicoId = osId;
        ServicoId = servicoId;
        NomeServico = nome;
        PrecoUnitario = preco;
        Quantidade = quantidade;
    }
}*/