using System.Text.Json.Serialization;

namespace Api.Controllers.OrdensServico.Requests;

public record InserirProdutosOrdemServicoRequest(
    [property: JsonPropertyName("produtos")] List<InserirProdutosItemRequest> Produtos);