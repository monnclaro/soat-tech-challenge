using System.Text.Json.Serialization;

namespace Api.Controllers.Clientes.Controllers.Requests;

public record InserirClienteRequest(
    [property: JsonPropertyName("nome")] string Nome,
    [property: JsonPropertyName("documento")] string Documento);