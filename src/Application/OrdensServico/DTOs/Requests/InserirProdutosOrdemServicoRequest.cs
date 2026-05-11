namespace Application.OrdensServico.DTOs.Requests;

public class InserirProdutosOrdemServicoRequest
{
    public List<InserirProdutosOrdemServicoProdutoRequest> Produtos { get; set; }

    public InserirProdutosOrdemServicoRequest(List<InserirProdutosOrdemServicoProdutoRequest> produtos)
    {
        Produtos = produtos;
    }
}

public class InserirProdutosOrdemServicoProdutoRequest
{
    public Guid IdProduto { get; set; }
    public decimal Quantidade { get; set; }

    public InserirProdutosOrdemServicoProdutoRequest(Guid idProduto, decimal quantidade)
    {
        IdProduto = idProduto;
        Quantidade = quantidade;
    }
}