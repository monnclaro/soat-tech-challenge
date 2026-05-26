using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Api.Controllers.OrdensServico.Requests;

public record InserirOrdemServicoCompletaRequest(
    [property: Required][property: JsonPropertyName("cliente")] InserirOrdemServicoCompletaClienteRequest Cliente,
    [property: Required][property: JsonPropertyName("servicos")] List<InserirOrdemServicoCompletaServicoRequest> Servicos,
    [property: Required][property: JsonPropertyName("produtos")] List<InserirOrdemServicoCompletaProdutoRequest> Produtos
);

public record InserirOrdemServicoCompletaClienteRequest(
    [property: Required][property: JsonPropertyName("nome")] string Nome,
    [property: Required][property: JsonPropertyName("documento")] string Documento,
    [property: Required][property: JsonPropertyName("veiculo")] InserirOrdemServicoCompletaVeiculoRequest Veiculo
);

public record InserirOrdemServicoCompletaVeiculoRequest(
    [property: Required][property: JsonPropertyName("placa")] string Placa,
    [property: Required][property: JsonPropertyName("marca")] string Marca,
    [property: Required][property: JsonPropertyName("modelo")] string Modelo,
    [property: Required][property: JsonPropertyName("ano")] int Ano
);

public record InserirOrdemServicoCompletaServicoRequest(
    [property: Required][property: JsonPropertyName("nome")] string Nome,
    [property: Required][property: JsonPropertyName("descricao")] string Descricao,
    [property: Required][property: JsonPropertyName("valor")] decimal Valor
);

public record InserirOrdemServicoCompletaProdutoRequest(
    [property: Required][property: JsonPropertyName("nome")] string Nome,
    [property: Required][property: JsonPropertyName("descricao")] string Descricao,
    [property: Required][property: JsonPropertyName("valor")] decimal Valor,
    [property: Required][property: JsonPropertyName("quantidadeEmEstoque")] int QuantidadeEmEstoque,
    [property: Required][property: JsonPropertyName("quantidadeNaOrdem")] int QuantidadeNaOrdem
);