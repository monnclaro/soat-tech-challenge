using System.Text.Json.Serialization;

namespace Api.Controllers.Clientes.Requests;

public record InserirClienteRequest(
    [property: JsonPropertyName("nome")] string Nome,
    [property: JsonPropertyName("documento")] string Documento);

public record AtualizarClienteRequest(
    [property: JsonPropertyName("nome")] string Nome);
