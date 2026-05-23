using System.Text.Json.Serialization;

namespace Api.Controllers.Clientes.Controllers.Requests;

public record AtualizarClienteRequest([property: JsonPropertyName("nome")] string Nome);