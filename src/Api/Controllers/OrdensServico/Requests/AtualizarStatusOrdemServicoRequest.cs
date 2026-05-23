using System.Text.Json.Serialization;
using Domain.OrdensServico.Enums;

namespace Api.Controllers.OrdensServico.Requests;

public record AtualizarStatusOrdemServicoRequest([property: JsonPropertyName("status")] StatusOrdemServico Status);