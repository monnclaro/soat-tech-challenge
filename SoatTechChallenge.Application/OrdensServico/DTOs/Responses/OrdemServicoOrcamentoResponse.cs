namespace SoatTechChallenge.Host.Controllers.OrdensServico.DTOs.Responses;

public record OrdemServicoOrcamentoResponse(
    Guid Id,
    decimal ValorTotal
);