using System.Text.Json.Serialization;
using Domain.OrdensServico.Enums;

namespace Api.Controllers.OrdensServico.Controllers.Requests;

public record AtualizarStatusOrdemServicoRequest([property: JsonPropertyName("status")] StatusOrdemServico Status);