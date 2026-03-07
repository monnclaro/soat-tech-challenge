namespace SoatTechChallenge.Host.Controllers.Servicos.DTOs;

public record InserirServicoRequest(  
    string Nome,
    string Descricao,
    decimal Preco,
    int TempoEstimadoMinutos);