namespace SoatTechChallenge.Application.Produtos.DTOs.Requests;

public class AtualizarQuantidadeEstoqueProdutoRequest
{
    public decimal Quantidade { get; set; }

    public AtualizarQuantidadeEstoqueProdutoRequest()
    {
        
    }

    public AtualizarQuantidadeEstoqueProdutoRequest(decimal quantidade)
    {
        Quantidade = quantidade;
    }
}