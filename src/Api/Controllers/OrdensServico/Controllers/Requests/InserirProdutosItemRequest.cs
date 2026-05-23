using System.Text.Json.Serialization;

namespace Api.Controllers.OrdensServico.Controllers.Requests;

public record InserirProdutosItemRequest(
    [property: JsonPropertyName("idProduto")] Guid IdProduto,
    [property: JsonPropertyName("quantidade")] int Quantidade);