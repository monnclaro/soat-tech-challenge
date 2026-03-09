namespace SoatTechChallenge.Host.Controllers.OrdensServico.DTOs.Responses;

public record OrdemServicoPorDocumentoResponse(
 OrdemServicoClientePorDocumentoResponse Cliente,
 OrdemServicoVeiculoPorDocumentoResponse Veiculo,
 DateTime DataCriacao,
 DateTime? DataInicioExecucao,
 DateTime? DataFinalizacao,
 string Status,
 decimal ValorTotal,
 List<OrdemServicoServicoPorDocumentoResponse> Servicos,
 List<OrdemServicoProdutoPorDocumentoResponse> Produtos
);

public record OrdemServicoClientePorDocumentoResponse(

 string Nome,
 string Documento
 );

public record OrdemServicoVeiculoPorDocumentoResponse(

 string Placa,
 string Marca,
 string Modelo,
 int Ano
);

public record OrdemServicoServicoPorDocumentoResponse(
 string NomeServico,
 decimal Valor
);

public record OrdemServicoProdutoPorDocumentoResponse(
 string NomeProduto,
 decimal ValorUnitario,
 decimal Quantidade
);