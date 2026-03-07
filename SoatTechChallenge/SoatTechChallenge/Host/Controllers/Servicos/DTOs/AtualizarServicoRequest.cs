namespace SoatTechChallenge.Host.Controllers.Servicos.DTOs;

public record AtualizarServicoRequest(
    string Nome,
    string Descricao,
    decimal Preco,
    int TempoEstimadoMinutos);