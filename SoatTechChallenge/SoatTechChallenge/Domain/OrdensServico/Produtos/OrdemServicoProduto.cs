namespace SoatTechChallenge.Domain.OrdensServico.Produtos;

public class OrdemServicoProduto
{
    public Guid Id { get; private set; }
    public Guid IdOrgemServico { get; private set; }
    public Guid IdPeca { get; private set; }
    public string NomePeca { get; private set; }
    public decimal PrecoUnitario { get; private set; }
    public int Quantidade { get; private set; }
    public decimal Subtotal => PrecoUnitario * Quantidade;

    public OrdemServicoProduto() { }
}