using System.Text.Json.Serialization;

namespace Api.Controllers.OrdensServico.Requests;

public record InserirOrdemServicoRequest(
    [property: JsonPropertyName("idCliente")] Guid IdCliente,
    [property: JsonPropertyName("idVeiculo")] Guid IdVeiculo,
    [property: JsonPropertyName("idsServicos")] List<Guid> IdsServicos,
    [property: JsonPropertyName("produtos")] List<InserirOrdemServicoProdutoRequest> Produtos);