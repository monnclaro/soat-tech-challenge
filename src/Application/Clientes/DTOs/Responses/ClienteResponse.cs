namespace Application.Clientes.DTOs.Responses;

public record ClienteResponse(
    Guid Id,
    string Nome,
    string Documento,
    DateTime DataCriacao
);