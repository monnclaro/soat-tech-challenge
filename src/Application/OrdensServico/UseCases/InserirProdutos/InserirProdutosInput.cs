namespace Application.OrdensServico.UseCases.InserirProdutos;

public record InserirProdutosInput(Guid IdOrdemServico, List<InserirProdutosItemInput> Produtos);
public record InserirProdutosItemInput(Guid IdProduto, decimal Quantidade);