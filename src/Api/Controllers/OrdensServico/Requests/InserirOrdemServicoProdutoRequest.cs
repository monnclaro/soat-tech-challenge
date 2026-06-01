using System.Text.Json.Serialization;

namespace Api.Controllers.OrdensServico.Requests;

public record InserirOrdemServicoProdutoRequest(
    [property: JsonPropertyName("idProduto")] Guid IdProduto,
    [property: JsonPropertyName("quantidade")] int Quantidade);