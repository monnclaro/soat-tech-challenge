using System.ComponentModel.DataAnnotations;

namespace SoatTechChallenge.Application.Servicos.DTOs.Requests;

public record AtualizarServicoRequest(  
    [Required]
    [MaxLength(100)]
    string Nome,

    [Required]
    [MaxLength(500)]
    string Descricao,

    [Range(0, double.MaxValue)]
    decimal Valor);