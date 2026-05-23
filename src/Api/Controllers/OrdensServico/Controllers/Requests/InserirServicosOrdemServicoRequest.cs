using System.Text.Json.Serialization;

namespace Api.Controllers.OrdensServico.Controllers.Requests;

public record InserirServicosOrdemServicoRequest(
    [property: JsonPropertyName("servicos")] List<InserirServicosItemRequest> Servicos);