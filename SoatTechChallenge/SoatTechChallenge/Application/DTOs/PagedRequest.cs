namespace SoatTechChallenge.Application.DTOs;

public record PagedRequest(int Pagina = 1, int Tamanho  = 25, string Filtro = "");