namespace SoatTechChallenge.Host.Controllers.OrdensServico.DTOs.Requests;

public record AtualizarOrdemServicoRequest(
    string Nome, 
    List<InserirOrdemServicoServicoRequest> Servicos, 
    List<InserirOrdemServicoProdutoRequest> Produtos);

public record AtualizarOrdemServicoServicoRequest(Guid IdServico, decimal Preco);
public record AtualizarOrdemServicoProdutoRequest(Guid IdProduto, decimal PrecoUnitario, decimal Quantidade);