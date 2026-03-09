namespace SoatTechChallenge.Host.Controllers.OrdensServico.DTOs.Requests;

public record AtualizarOrdemServicoRequest(
    List<InserirOrdemServicoServicoRequest> Servicos, 
    List<InserirOrdemServicoProdutoRequest> Produtos);

public record AtualizarOrdemServicoServicoRequest(Guid IdServico);
public record AtualizarOrdemServicoProdutoRequest(Guid IdProduto, decimal Quantidade);