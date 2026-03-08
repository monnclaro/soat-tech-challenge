namespace SoatTechChallenge.Host.Controllers.OrdensServico.DTOs.Requests;

public record InserirOrdemServicoRequest(
    Guid IdCliente,
    Guid IdVeiculo, 
    List<InserirOrdemServicoServicoRequest> Servicos, 
    List<InserirOrdemServicoProdutoRequest> Produtos);

public record InserirOrdemServicoServicoRequest(Guid IdServico);
public record InserirOrdemServicoProdutoRequest(Guid IdProduto, decimal Quantidade);