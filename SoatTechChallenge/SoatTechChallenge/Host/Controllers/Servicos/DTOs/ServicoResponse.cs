namespace SoatTechChallenge.Host.Controllers.Servicos.DTOs;

public record ServicoResponse(
  Guid Id,
  string Nome,
  string Descricao,
  decimal Preco
);