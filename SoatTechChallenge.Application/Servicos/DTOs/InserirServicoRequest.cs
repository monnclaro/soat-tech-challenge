namespace SoatTechChallenge.Application.Servicos.DTOs;

public record InserirServicoRequest(  
    string Nome,
    string Descricao,
    decimal Valor);