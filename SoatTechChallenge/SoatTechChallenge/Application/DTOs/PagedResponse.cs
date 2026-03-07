namespace SoatTechChallenge.Application.DTOs;

public record PagedResponse<T>(IReadOnlyCollection<T> Itens);