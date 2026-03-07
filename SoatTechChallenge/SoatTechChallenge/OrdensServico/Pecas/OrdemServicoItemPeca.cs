namespace SoatTechChallenge.OrdensServico.Pecas;

public class OrdemServicoItemPeca
{
    public Guid Id { get; private set; }
    public Guid OrdemServicoId { get; private set; }
    public Guid PecaId { get; private set; }
    public string NomePeca { get; private set; }
    public decimal PrecoUnitario { get; private set; }
    public int Quantidade { get; private set; }
    public decimal Subtotal => PrecoUnitario * Quantidade;

    protected OrdemServicoItemPeca() { }

    public OrdemServicoItemPeca(Guid osId, Guid pecaId, string nome, decimal preco, int quantidade)
    {
        Id = Guid.NewGuid();
        OrdemServicoId = osId;
        PecaId = pecaId;
        NomePeca = nome;
        PrecoUnitario = preco;
        Quantidade = quantidade;
    }
}