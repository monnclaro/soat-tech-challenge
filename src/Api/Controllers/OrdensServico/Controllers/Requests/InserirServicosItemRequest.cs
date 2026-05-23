using System.Text.Json.Serialization;

namespace Api.Controllers.OrdensServico.Controllers.Requests;

public record InserirServicosItemRequest(
    [property: JsonPropertyName("idServico")] Guid IdServico);