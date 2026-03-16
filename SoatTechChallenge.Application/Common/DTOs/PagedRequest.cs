namespace SoatTechChallenge.Application.Common.DTOs;

public record PagedRequest(int Pagina = 1, int Tamanho = 25);