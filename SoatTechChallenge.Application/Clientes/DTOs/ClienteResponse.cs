namespace SoatTechChallenge.Application.Clientes.DTOs;

public record ClienteResponse(
    Guid Id,
    string Nome,
    string Documento,
    DateTime DataCriacao
);