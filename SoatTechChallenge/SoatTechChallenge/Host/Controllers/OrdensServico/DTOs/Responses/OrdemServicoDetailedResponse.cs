namespace SoatTechChallenge.Host.Controllers.OrdensServico.DTOs.Responses;

public record OrdemServicoDetailedResponse(
 Guid Id,
 OrdemServicoClienteDetailedResponse Cliente,
 OrdemServicoVeiculoDetailedResponse Veiculo,
 DateTime DataCriacao,
 DateTime? DataInicioExecucao,
 DateTime? DataFinalizacao,
 string Status,
 decimal ValorTotal,
 List<OrdemServicoServicoDetailedResponse> Servicos,
 List<OrdemServicoProdutoDetailedResponse> Produtos
);

public record OrdemServicoClienteDetailedResponse(
 Guid Id,
 string Nome,
 string Documento
 );

public record OrdemServicoVeiculoDetailedResponse(
 Guid Id,
 string Placa,
 string Marca,
 string Modelo,
 int Ano
);

public record OrdemServicoServicoDetailedResponse(
 Guid Id,
 Guid IdServico,
 string NomeServico,
 decimal Valor
);

public record OrdemServicoProdutoDetailedResponse(
 Guid Id,
 Guid IdProduto,
 string NomeProduto,
 decimal ValorUnitario,
 decimal Quantidade
);
