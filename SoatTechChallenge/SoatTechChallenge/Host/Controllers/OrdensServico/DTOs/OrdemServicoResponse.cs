namespace SoatTechChallenge.Host.Controllers.Clientes.DTOs;

public record OrdemServicoResponse(
    Guid Id,
    string Nome,
    string Documento,
    DateTime DataCriacao
);