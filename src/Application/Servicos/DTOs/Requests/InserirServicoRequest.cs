using System.ComponentModel.DataAnnotations;

namespace Application.Servicos.DTOs.Requests;

public record InserirServicoRequest(
    [Required]
    [MaxLength(100)]
    string Nome,

    [Required]
    [MaxLength(500)]
    string Descricao,

    [Range(0, double.MaxValue)]
    decimal Valor);