using System.Text.Json.Serialization;

namespace Api.Controllers.OrdensServico.Controllers.Requests;

public record InserirProdutosOrdemServicoRequest(
    [property: JsonPropertyName("produtos")] List<InserirProdutosItemRequest> Produtos);