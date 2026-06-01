using System.Text.Json.Serialization;

namespace Api.Controllers.OrdensServico.Requests;

public record InserirServicosOrdemServicoRequest(
    [property: JsonPropertyName("servicos")] List<InserirServicosItemRequest> Servicos);