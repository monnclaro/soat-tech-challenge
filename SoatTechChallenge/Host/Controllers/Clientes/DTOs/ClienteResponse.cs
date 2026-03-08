namespace SoatTechChallenge.Host.Controllers.Clientes.DTOs;

public record ClienteResponse(
    Guid Id,
    string Nome,
    string Documento,
    DateTime DataCriacao
);