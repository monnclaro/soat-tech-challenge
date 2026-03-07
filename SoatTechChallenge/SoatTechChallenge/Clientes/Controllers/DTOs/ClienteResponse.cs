namespace SoatTechChallenge.Clientes.Controllers.DTOs;

public record ClienteResponse(
    Guid Id,
    string Nome,
    string Documento,
    DateTime DataCriacao
);