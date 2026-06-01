using System.Text.Json.Serialization;

namespace Api.Controllers.Clientes.Requests;

public record AtualizarClienteRequest([property: JsonPropertyName("nome")] string Nome);