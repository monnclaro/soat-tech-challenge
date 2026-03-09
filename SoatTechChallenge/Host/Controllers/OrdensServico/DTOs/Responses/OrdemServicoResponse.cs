namespace SoatTechChallenge.Host.Controllers.OrdensServico.DTOs.Responses;

public record OrdemServicoResponse(
 Guid Id,
 OrdemServicoClienteResponse Cliente,
 OrdemServicoVeiculoResponse Veiculo,
 DateTime DataCriacao,
 DateTime? DataInicioExecucao,
 DateTime? DataFinalizacao,
 string Status,
 decimal ValorTotal,
 List<OrdemServicoServicoResponse> Servicos,
 List<OrdemServicoProdutoResponse> Produtos
);

public record OrdemServicoClienteResponse(
 Guid Id,
 string Nome,
 string Documento
 );

public record OrdemServicoVeiculoResponse(
 Guid Id,
 string Placa,
 string Marca,
 string Modelo,
 int Ano
);

public record OrdemServicoServicoResponse(
 Guid Id,
 Guid IdServico,
 string NomeServico,
 decimal Valor
);

public record OrdemServicoProdutoResponse(
 Guid Id,
 Guid IdProduto,
 string NomeProduto,
 decimal ValorUnitario,
 decimal Quantidade
);