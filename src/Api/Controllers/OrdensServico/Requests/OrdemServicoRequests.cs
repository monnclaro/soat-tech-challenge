using System.Text.Json.Serialization;

namespace Api.Controllers.OrdensServico.Requests;

public record InserirOrdemServicoProdutoRequest(
    [property: JsonPropertyName("idProduto")] Guid IdProduto,
    [property: JsonPropertyName("quantidade")] int Quantidade);

public record InserirOrdemServicoRequest(
    [property: JsonPropertyName("idCliente")] Guid IdCliente,
    [property: JsonPropertyName("idVeiculo")] Guid IdVeiculo,
    [property: JsonPropertyName("idsServicos")] List<Guid> IdsServicos,
    [property: JsonPropertyName("produtos")] List<InserirOrdemServicoProdutoRequest> Produtos);

public record InserirProdutosItemRequest(
    [property: JsonPropertyName("idProduto")] Guid IdProduto,
    [property: JsonPropertyName("quantidade")] int Quantidade);

public record InserirProdutosOrdemServicoRequest(
    [property: JsonPropertyName("produtos")] List<InserirProdutosItemRequest> Produtos);

public record InserirServicosItemRequest(
    [property: JsonPropertyName("idServico")] Guid IdServico);

public record InserirServicosOrdemServicoRequest(
    [property: JsonPropertyName("servicos")] List<InserirServicosItemRequest> Servicos);
