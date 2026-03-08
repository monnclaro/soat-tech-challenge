namespace SoatTechChallenge.Host.Controllers.OrdensServico.DTOs.Responses;

public record OrdemServicoResponse(
 Guid Id,
 Guid IdCliente,
 Guid IdVeiculo,
 DateTime DataCriacao,
 DateTime? DataFinalizacao,
 string Status,
 decimal ValorTotal,
 List<OrdemServicoServicoResponse> Servicos,
 List<OrdemServicoProdutoResponse> Produtos
);

public record OrdemServicoServicoResponse(
 Guid Id,
 Guid IdServico,
 decimal Valor
);

public record OrdemServicoProdutoResponse(
 Guid Id,
 Guid IdProduto,
 string NomeProduto,
 decimal ValorUnitario,
 decimal Quantidade
);

